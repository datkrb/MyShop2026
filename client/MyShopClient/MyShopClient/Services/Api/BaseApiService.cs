using MyShopClient.Services.Config;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyShopClient.Services.Api;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; } = default!;
    public string? Error { get; set; }
    public string? Message { get; set; }
}

public abstract class BaseApiService
{
    protected readonly HttpClient _httpClient;
    protected readonly string _baseUrl; // This will now be set by the constructor or derived classes

    // Static URL shared across all API services
    protected static string _currentBaseUrl = string.Empty;
    
    /// <summary>
    /// Check if server URL has been configured
    /// </summary>
    public static bool IsConfigured => !string.IsNullOrEmpty(_currentBaseUrl);

    // Static token shared across all API services
    protected static string _currentToken = string.Empty;

    protected BaseApiService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _baseUrl = _currentBaseUrl; // Initialize _baseUrl for this instance from the static _currentBaseUrl
        
        // Apply existing token if available to the injected HttpClient
        if (!string.IsNullOrEmpty(_currentToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _currentToken);
        }
    }

    /// <summary>
    /// Initialize base URL from LocalSettings on app startup
    /// </summary>
    public static void InitializeBaseUrl()
    {
        var settingsService = new AppSettingsService();
        var url = settingsService.GetBaseUrl();
        
        // Ensure trailing slash
        SetBaseUrl(url.EndsWith("/") ? url : url + "/");

        System.Diagnostics.Debug.WriteLine($"[BaseApiService] Initialized Base URL: {_currentBaseUrl}");
    }

    /// <summary>
    /// Update base URL at runtime
    /// </summary>
    public static void SetBaseUrl(string url)
    {
        _currentBaseUrl = url;
        if (!_currentBaseUrl.EndsWith("/")) _currentBaseUrl += "/";
    }

    public static void UpdateBaseUrl(string url) => SetBaseUrl(url);

    protected void SetAuthToken(string token)
    {
        _currentToken = token; // fixed
        if (_httpClient != null)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public static void ClearAuthToken()
    {
        _currentToken = null; // fixed
    }

    /// <summary>
    /// Apply the current static token to this instance's HttpClient.
    /// Call this after login to update existing singleton instances.
    /// </summary>
    public void ApplyCurrentToken()
    {
        if (_httpClient == null) return;

        if (!string.IsNullOrEmpty(_currentToken)) // fixed
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _currentToken); // fixed
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    /// <summary>
    /// Apply the current static base URL to this instance's HttpClient.
    /// Call this after saving new URL to update existing singleton instances.
    /// </summary>
    public void ApplyCurrentBaseUrl()
    {
        if (_httpClient == null) return;

        if (_httpClient.BaseAddress?.ToString() != _currentBaseUrl)
        {
            // Note: HttpClient.BaseAddress can only be set once, so we need to recreate the client
            // For now, we'll just update our _baseUrl reference - new requests will use new URL through full URL construction
            System.Diagnostics.Debug.WriteLine($"Base URL updated to: {_currentBaseUrl}");
        }
    }

    private T? ParseResponse<T>(string content)
    {
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        if (apiResponse == null) return default;

        if (!apiResponse.Success)
        {
            throw new Exception(apiResponse.Message ?? apiResponse.Error ?? "Unknown error");
        }

        return apiResponse.Data;
    }

    private void HandleException(Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        
        if (ex is TaskCanceledException)
        {
            throw new Exception("Request timeout. Please check your internet connection.", ex);
        }
        
        if (ex is JsonException)
        {
            throw new Exception("Failed to parse server response.", ex);
        }
        
        throw ex;
    }

    private async Task<bool> TryRefreshTokenAndRetryAsync()
    {
        try
        {
            // Resolve CredentialService via Service Locator to avoid breaking constructor signatures
            var credentialService = Microsoft.UI.Xaml.Application.Current is MyShopClient.App app 
                ? Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<Services.Auth.CredentialService>(app.Services) 
                : null;

            if (credentialService == null) return false;

            var tokens = credentialService.GetTokens();
            if (tokens == null) return false;

            // Call Refresh Endpoint manually to avoid circular dependency with AuthApiService
            var refreshRequest = new { refreshToken = tokens.Value.RefreshToken };
            var json = JsonSerializer.Serialize(refreshRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var fullUrl = _currentBaseUrl + "auth/refresh-token";
            var response = await _httpClient.PostAsync(fullUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<ApiResponse<Models.LoginResponse>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (loginResponse != null && loginResponse.Success && loginResponse.Data != null)
                {
                    var newAccessToken = loginResponse.Data.AccessToken;
                    // Note: If backend rotates RefreshToken, update it here too. Currently it just returns AccessToken.
                    
                    credentialService.SaveTokens(newAccessToken, tokens.Value.RefreshToken);
                    SetAuthToken(newAccessToken);
                    ApplyCurrentToken();
                    
                    System.Diagnostics.Debug.WriteLine("Token refreshed successfully.");
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Token refresh failed: {ex.Message}");
        }

        return false;
    }

    private void HandleSessionExpired()
    {
        // Force Logout
        ClearAuthToken();
        
        // Clear saved tokens via CredentialService
        if (Microsoft.UI.Xaml.Application.Current is MyShopClient.App app)
        {
             var credentialService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<Services.Auth.CredentialService>(app.Services);
             credentialService?.ClearTokens();

             // Navigate to Login using DispatcherQueue to ensure UI thread access
             if (app.MainWindow != null)
             {
                 app.MainWindow.DispatcherQueue.TryEnqueue(() =>
                 {
                     app.RootFrame?.Navigate(typeof(Views.Login.LoginView));
                 });
             }
        }
        
        System.Diagnostics.Debug.WriteLine("Session expired. Redirecting to Login.");
    }

    protected async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            ApplyCurrentToken();
            var fullUrl = _currentBaseUrl + endpoint;
            System.Diagnostics.Debug.WriteLine($"GET {fullUrl}");
            
            var response = await _httpClient.GetAsync(fullUrl);
            
            // Handle 401
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                System.Diagnostics.Debug.WriteLine("401 Unauthorized - Attempting Refresh...");
                if (await TryRefreshTokenAndRetryAsync())
                {
                    // Retry
                    response = await _httpClient.GetAsync(fullUrl);
                }
                else
                {
                    HandleSessionExpired();
                    return default;
                }
            }

            var content = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Response [{response.StatusCode}]: {content}");
            return ParseResponse<T>(content);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return default;
        }
    }

    protected async Task<T?> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            ApplyCurrentToken();
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var fullUrl = _currentBaseUrl + endpoint;
            System.Diagnostics.Debug.WriteLine($"POST {fullUrl}");
            
            var response = await _httpClient.PostAsync(fullUrl, content);

             // Handle 401
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                System.Diagnostics.Debug.WriteLine("401 Unauthorized - Attempting Refresh...");
                if (await TryRefreshTokenAndRetryAsync())
                {
                    // Re-create content and Retry
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                    response = await _httpClient.PostAsync(fullUrl, content);
                }
                 else
                {
                    HandleSessionExpired();
                    return default;
                }
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Response [{response.StatusCode}]: {responseContent}");
            return ParseResponse<T>(responseContent);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return default;
        }
    }

    protected async Task<T?> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            ApplyCurrentToken();
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var fullUrl = _currentBaseUrl + endpoint;
            System.Diagnostics.Debug.WriteLine($"PUT {fullUrl}");
            
            var response = await _httpClient.PutAsync(fullUrl, content);

             // Handle 401
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                System.Diagnostics.Debug.WriteLine("401 Unauthorized - Attempting Refresh...");
                if (await TryRefreshTokenAndRetryAsync())
                {
                    // Re-create content and Retry
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                    response = await _httpClient.PutAsync(fullUrl, content);
                }
                 else
                {
                    HandleSessionExpired();
                    return default;
                }
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Response [{response.StatusCode}]: {responseContent}");
            return ParseResponse<T>(responseContent);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return default;
        }
    }

    protected async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            ApplyCurrentToken();
            var fullUrl = _currentBaseUrl + endpoint;
            System.Diagnostics.Debug.WriteLine($"DELETE {fullUrl}");
            
            var response = await _httpClient.DeleteAsync(fullUrl);

             // Handle 401
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                System.Diagnostics.Debug.WriteLine("401 Unauthorized - Attempting Refresh...");
                if (await TryRefreshTokenAndRetryAsync())
                {
                    // Retry
                    response = await _httpClient.DeleteAsync(fullUrl);
                }
                 else
                {
                    HandleSessionExpired();
                    return false;
                }
            }

            var content = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Response [{response.StatusCode}]: {content}");
            
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (apiResponse != null && !apiResponse.Success)
            {
                throw new Exception(apiResponse.Message ?? apiResponse.Error ?? "Delete failed");
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Delete Error: {ex.Message}");
            return false;
        }
    }
}

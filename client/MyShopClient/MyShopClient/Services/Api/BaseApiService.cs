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
        var url = settingsService.GetServerUrl();
        var port = settingsService.GetServerPort();
        
        // Remove trailing slash if present
        if (url.EndsWith("/")) url = url.Substring(0, url.Length - 1);
        
        SetBaseUrl($"{url}:{port}/api/");

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

    protected async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            // Always apply current token before request
            ApplyCurrentToken();
            
            // Use full URL with current base URL (allows dynamic URL changes)
            var fullUrl = _currentBaseUrl + endpoint;
            System.Diagnostics.Debug.WriteLine($"GET {fullUrl}");
            
            var response = await _httpClient.GetAsync(fullUrl);
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
            // Always apply current token before request
            ApplyCurrentToken();
            
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Use full URL with current base URL (allows dynamic URL changes)
            var fullUrl = _currentBaseUrl + endpoint;
            System.Diagnostics.Debug.WriteLine($"POST {fullUrl}");
            System.Diagnostics.Debug.WriteLine($"Request Body: {json}");
            
            var response = await _httpClient.PostAsync(fullUrl, content);
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
            // Always apply current token before request
            ApplyCurrentToken();
            
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Use full URL with current base URL (allows dynamic URL changes)
            var fullUrl = _currentBaseUrl + endpoint;
            System.Diagnostics.Debug.WriteLine($"PUT {fullUrl}");
            System.Diagnostics.Debug.WriteLine($"Request Body: {json}");
            
            var response = await _httpClient.PutAsync(fullUrl, content);
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
            // Always apply current token before request
            ApplyCurrentToken();
            
            // Use full URL with current base URL (allows dynamic URL changes)
            var fullUrl = _currentBaseUrl + endpoint;
            System.Diagnostics.Debug.WriteLine($"DELETE {fullUrl}");
            
            var response = await _httpClient.DeleteAsync(fullUrl);
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

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
    protected readonly string _baseUrl = "http://localhost:3000/api/";

    // Static token shared across all API services
    public static string? CurrentToken { get; private set; }

    protected BaseApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl),
            Timeout = TimeSpan.FromSeconds(10)
        };
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        // Apply existing token if available
        if (!string.IsNullOrEmpty(CurrentToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CurrentToken);
        }
    }

    protected void SetAuthToken(string token)
    {
        CurrentToken = token;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static void ClearAuthToken()
    {
        CurrentToken = null;
    }

    /// <summary>
    /// Apply the current static token to this instance's HttpClient.
    /// Call this after login to update existing singleton instances.
    /// </summary>
    public void ApplyCurrentToken()
    {
        if (!string.IsNullOrEmpty(CurrentToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CurrentToken);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
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
            
            System.Diagnostics.Debug.WriteLine($"GET {_baseUrl}{endpoint}");
            
            var response = await _httpClient.GetAsync(endpoint);
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
            
            System.Diagnostics.Debug.WriteLine($"POST {_baseUrl}{endpoint}");
            System.Diagnostics.Debug.WriteLine($"Request Body: {json}");
            
            var response = await _httpClient.PostAsync(endpoint, content);
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
            
            System.Diagnostics.Debug.WriteLine($"PUT {_baseUrl}{endpoint}");
            System.Diagnostics.Debug.WriteLine($"Request Body: {json}");
            
            var response = await _httpClient.PutAsync(endpoint, content);
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
            
            System.Diagnostics.Debug.WriteLine($"DELETE {_baseUrl}{endpoint}");
            
            var response = await _httpClient.DeleteAsync(endpoint);
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

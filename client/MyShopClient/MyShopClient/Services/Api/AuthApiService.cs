using System.Threading.Tasks;
using MyShopClient.Models;

namespace MyShopClient.Services.Api;

public class AuthApiService : BaseApiService
{
    private static AuthApiService? _instance;
    public static AuthApiService Instance => _instance ??= new AuthApiService();

    public async Task<LoginResponse?> LoginAsync(string username, string password)
    {
        var request = new LoginRequest
        {
            Username = username,
            Password = password
        };

        var response = await PostAsync<LoginResponse>("auth/login", request);
        
        if (response != null && !string.IsNullOrEmpty(response.Token))
        {
            SetAuthToken(response.Token);
        }

        return response;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        return await GetAsync<User>("auth/me");
    }

    public async Task<bool> LogoutAsync()
    {
        try
        {
            await PostAsync<object>("auth/logout", new { });
            
            // Clear token
            _httpClient.DefaultRequestHeaders.Authorization = null;
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void SetToken(string token)
    {
        SetAuthToken(token);
    }

    public void ClearToken()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
}

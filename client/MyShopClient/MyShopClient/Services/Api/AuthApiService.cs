using System.Threading.Tasks;
using System.Net.Http;
using MyShopClient.Models;

namespace MyShopClient.Services.Api;

public class AuthApiService : BaseApiService
{
    public AuthApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<LoginResponse?> LoginAsync(string username, string password)
    {
        var request = new LoginRequest
        {
            Username = username,
            Password = password
        };

        var response = await PostAsync<LoginResponse>("auth/login", request);
        
        if (response != null && !string.IsNullOrEmpty(response.AccessToken))
        {
            SetAuthToken(response.AccessToken);
        }

        return response;
    }

    public async Task<LoginResponse?> RefreshTokenAsync(string refreshToken)
    {
        var response = await PostAsync<LoginResponse>("auth/refresh-token", new { refreshToken });
        
        if (response != null && !string.IsNullOrEmpty(response.AccessToken))
        {
             // Note: Refresh API returns new AccessToken. 
             // If backend returned new RefreshToken, we would update it here too.
             // Based on our implementation plan, backend returns { accessToken, expiresIn } but using LoginResponse structure.
             SetAuthToken(response.AccessToken);
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
            ClearToken();
            
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

    public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        try
        {
            var response = await PostAsync<object>("auth/change-password", new 
            { 
                currentPassword, 
                newPassword 
            });
            return true;
        }
        catch
        {
            return false;
        }
    }
}

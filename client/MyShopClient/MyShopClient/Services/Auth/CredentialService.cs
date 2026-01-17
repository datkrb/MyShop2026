using System;
using Windows.Security.Credentials;

namespace MyShopClient.Services.Auth;

/// <summary>
/// Service để quản lý credentials sử dụng Windows PasswordVault (mã hóa AES256)
/// </summary>
public class CredentialService
{
    private const string ResourceName = "MyShopClient_Tokens";
    private const string AccessTokenKey = "AccessToken";
    private const string RefreshTokenKey = "RefreshToken";

    /// <summary>
    /// Lưu tokens bảo mật vào PasswordVault
    /// </summary>
    public void SaveTokens(string accessToken, string refreshToken)
    {
        try
        {
            var vault = new PasswordVault();
            
            // Xóa cũ trước khi lưu mới
            ClearTokens();

            // Lưu Access Token
            vault.Add(new PasswordCredential(ResourceName, AccessTokenKey, accessToken));
            // Lưu Refresh Token
            vault.Add(new PasswordCredential(ResourceName, RefreshTokenKey, refreshToken));
            
            System.Diagnostics.Debug.WriteLine("Tokens saved successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving tokens: {ex.Message}");
        }
    }

    /// <summary>
    /// Lấy Access và Refresh Token đã lưu
    /// </summary>
    public (string AccessToken, string RefreshToken)? GetTokens()
    {
        try
        {
            var vault = new PasswordVault();
            var credentials = vault.FindAllByResource(ResourceName);

            string? accessToken = null;
            string? refreshToken = null;

            foreach (var cred in credentials)
            {
                cred.RetrievePassword();
                if (cred.UserName == AccessTokenKey) accessToken = cred.Password;
                if (cred.UserName == RefreshTokenKey) refreshToken = cred.Password;
            }

            if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
            {
                return (accessToken, refreshToken);
            }
        }
        catch { }

        return null;
    }

    /// <summary>
    /// Xóa toàn bộ token
    /// </summary>
    public void ClearTokens()
    {
        try
        {
            var vault = new PasswordVault();
            var credentials = vault.FindAllByResource(ResourceName);
            foreach (var c in credentials) vault.Remove(c);
        }
        catch { }
    }

    public bool HasSavedTokens()
    {
        var tokens = GetTokens();
        return tokens != null;
    }
}

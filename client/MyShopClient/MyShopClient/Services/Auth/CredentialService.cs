using System;
using Windows.Security.Credentials;

namespace MyShopClient.Services.Auth;

/// <summary>
/// Service để quản lý credentials sử dụng Windows PasswordVault (mã hóa AES256)
/// </summary>
public class CredentialService
{
    private const string ResourceName = "MyShopClient";

    /// <summary>
    /// Lưu credentials vào PasswordVault (mã hóa tự động)
    /// </summary>
    public void SaveCredentials(string username, string password)
    {
        try
        {
            // Xóa credentials cũ trước
            ClearCredentials();

            var vault = new PasswordVault();
            var credential = new PasswordCredential(ResourceName, username, password);
            vault.Add(credential);
            
            System.Diagnostics.Debug.WriteLine($"Credentials saved for user: {username}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving credentials: {ex.Message}");
        }
    }

    /// <summary>
    /// Lấy credentials đã lưu từ PasswordVault
    /// </summary>
    /// <returns>Tuple (username, password) hoặc null nếu không có</returns>
    public (string Username, string Password)? GetCredentials()
    {
        try
        {
            var vault = new PasswordVault();
            var credentials = vault.FindAllByResource(ResourceName);

            if (credentials.Count > 0)
            {
                var credential = credentials[0];
                credential.RetrievePassword(); // Cần gọi để lấy password
                
                System.Diagnostics.Debug.WriteLine($"Credentials found for user: {credential.UserName}");
                return (credential.UserName, credential.Password);
            }
        }
        catch (Exception ex)
        {
            // FindAllByResource throws exception if no credentials found
            System.Diagnostics.Debug.WriteLine($"No credentials found: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Xóa tất cả credentials đã lưu
    /// </summary>
    public void ClearCredentials()
    {
        try
        {
            var vault = new PasswordVault();
            var credentials = vault.FindAllByResource(ResourceName);

            foreach (var credential in credentials)
            {
                vault.Remove(credential);
            }
            
            System.Diagnostics.Debug.WriteLine("Credentials cleared");
        }
        catch (Exception ex)
        {
            // Ignore if no credentials to clear
            System.Diagnostics.Debug.WriteLine($"No credentials to clear: {ex.Message}");
        }
    }

    /// <summary>
    /// Kiểm tra xem có credentials đã lưu không
    /// </summary>
    public bool HasSavedCredentials()
    {
        try
        {
            var vault = new PasswordVault();
            var credentials = vault.FindAllByResource(ResourceName);
            return credentials.Count > 0;
        }
        catch
        {
            return false;
        }
    }
}

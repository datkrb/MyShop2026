using System;

namespace MyShopClient.Services.Config;

/// <summary>
/// Service quản lý các cài đặt chung của ứng dụng (lưu vào LocalSettings)
/// </summary>
public class AppSettingsService
{
    private const string PageSizeKey = "PageSize";
    private const string RememberLastScreenKey = "RememberLastScreen";
    private const string DefaultScreenKey = "DefaultScreen";
    
    private const int DefaultPageSize = 10;
    private const bool DefaultRememberLastScreen = true;
    private const string DefaultDefaultScreen = "Dashboard";

    #region PageSize

    /// <summary>
    /// Lấy số dòng mỗi trang từ Settings
    /// </summary>
    public int GetPageSize()
    {
        try
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var value = localSettings.Values[PageSizeKey];
            if (value is int pageSize && pageSize > 0)
            {
                return pageSize;
            }
        }
        catch
        {
            // Ignore errors
        }
        return DefaultPageSize;
    }

    /// <summary>
    /// Lưu số dòng mỗi trang vào Settings
    /// </summary>
    public void SavePageSize(int pageSize)
    {
        try
        {
            if (pageSize <= 0) return;
            
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[PageSizeKey] = pageSize;
            
            System.Diagnostics.Debug.WriteLine($"PageSize saved: {pageSize}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving PageSize: {ex.Message}");
        }
    }

    #endregion

    #region RememberLastScreen

    /// <summary>
    /// Lấy cài đặt Remember Last Screen
    /// </summary>
    public bool GetRememberLastScreen()
    {
        try
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var value = localSettings.Values[RememberLastScreenKey];
            if (value is bool remember)
            {
                return remember;
            }
        }
        catch
        {
            // Ignore errors
        }
        return DefaultRememberLastScreen;
    }

    /// <summary>
    /// Lưu cài đặt Remember Last Screen
    /// </summary>
    public void SaveRememberLastScreen(bool remember)
    {
        try
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[RememberLastScreenKey] = remember;
            
            System.Diagnostics.Debug.WriteLine($"RememberLastScreen saved: {remember}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving RememberLastScreen: {ex.Message}");
        }
    }

    #endregion

    #region DefaultScreen

    /// <summary>
    /// Lấy màn hình mặc định
    /// </summary>
    public string GetDefaultScreen()
    {
        try
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var value = localSettings.Values[DefaultScreenKey];
            if (value is string screen && !string.IsNullOrEmpty(screen))
            {
                return screen;
            }
        }
        catch
        {
            // Ignore errors
        }
        return DefaultDefaultScreen;
    }

    /// <summary>
    /// Lưu màn hình mặc định
    /// </summary>
    public void SaveDefaultScreen(string screen)
    {
        try
        {
            if (string.IsNullOrEmpty(screen)) return;
            
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[DefaultScreenKey] = screen;
            
            System.Diagnostics.Debug.WriteLine($"DefaultScreen saved: {screen}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving DefaultScreen: {ex.Message}");
        }
    }

    #endregion
}

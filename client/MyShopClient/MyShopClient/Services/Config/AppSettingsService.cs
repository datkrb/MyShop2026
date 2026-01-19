using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MyShopClient.Services.Config;

/// <summary>
/// Service quản lý các cài đặt chung của ứng dụng (lưu vào file JSON cho unpackaged app)
/// </summary>
public class AppSettingsService
{
    private const string SettingsFileName = "appsettings.json";
    
    private const string PageSizeKey = "PageSize";
    private const string RememberLastScreenKey = "RememberLastScreen";
    private const string DefaultScreenKey = "DefaultScreen";
    private const string BaseUrlKey = "BaseUrl";
    
    private const int DefaultPageSize = 10;
    private const bool DefaultRememberLastScreen = true;
    private const string DefaultDefaultScreen = "Dashboard";
    private const string DefaultBaseUrl = "http://localhost:3000";

    private readonly string _settingsFolder;
    private readonly string _settingsFilePath;

    public AppSettingsService()
    {
        // Lấy thư mục của ứng dụng (cùng cấp với .exe)
        _settingsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MyShopClient"
        );
        _settingsFilePath = Path.Combine(_settingsFolder, SettingsFileName);
        
        // Đảm bảo thư mục tồn tại
        EnsureDirectoryExists();
    }

    private void EnsureDirectoryExists()
    {
        try
        {
            if (!Directory.Exists(_settingsFolder))
            {
                Directory.CreateDirectory(_settingsFolder);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating settings directory: {ex.Message}");
        }
    }

    private Dictionary<string, object> LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    return JsonSerializer.Deserialize<Dictionary<string, object>>(json) 
                           ?? new Dictionary<string, object>();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
        }
        return new Dictionary<string, object>();
    }

    private void SaveSettings(Dictionary<string, object> settings)
    {
        try
        {
            EnsureDirectoryExists();
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
        }
    }

    private T GetValue<T>(string key, T defaultValue)
    {
        try
        {
            var settings = LoadSettings();
            if (settings.TryGetValue(key, out var value))
            {
                if (value is JsonElement jsonElement)
                {
                    if (typeof(T) == typeof(int) && jsonElement.TryGetInt32(out var intValue))
                    {
                        return (T)(object)intValue;
                    }
                    if (typeof(T) == typeof(bool) && jsonElement.ValueKind == JsonValueKind.True)
                    {
                        return (T)(object)true;
                    }
                    if (typeof(T) == typeof(bool) && jsonElement.ValueKind == JsonValueKind.False)
                    {
                        return (T)(object)false;
                    }
                    if (typeof(T) == typeof(string))
                    {
                        return (T)(object)jsonElement.GetString()!;
                    }
                }
                else if (value is T typedValue)
                {
                    return typedValue;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting value for {key}: {ex.Message}");
        }
        return defaultValue;
    }

    private void SetValue(string key, object value)
    {
        try
        {
            var settings = LoadSettings();
            settings[key] = value;
            SaveSettings(settings);
            System.Diagnostics.Debug.WriteLine($"{key} saved: {value}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting value for {key}: {ex.Message}");
        }
    }

    #region PageSize

    /// <summary>
    /// Lấy số dòng mỗi trang từ Settings
    /// </summary>
    public int GetPageSize()
    {
        var value = GetValue(PageSizeKey, DefaultPageSize);
        return value > 0 ? value : DefaultPageSize;
    }

    /// <summary>
    /// Lưu số dòng mỗi trang vào Settings
    /// </summary>
    public void SavePageSize(int pageSize)
    {
        if (pageSize <= 0) return;
        SetValue(PageSizeKey, pageSize);
    }

    #endregion

    #region RememberLastScreen

    /// <summary>
    /// Lấy cài đặt Remember Last Screen
    /// </summary>
    public bool GetRememberLastScreen()
    {
        return GetValue(RememberLastScreenKey, DefaultRememberLastScreen);
    }

    /// <summary>
    /// Lưu cài đặt Remember Last Screen
    /// </summary>
    public void SaveRememberLastScreen(bool remember)
    {
        SetValue(RememberLastScreenKey, remember);
    }

    #endregion

    #region DefaultScreen

    /// <summary>
    /// Lấy màn hình mặc định
    /// </summary>
    public string GetDefaultScreen()
    {
        var value = GetValue(DefaultScreenKey, DefaultDefaultScreen);
        return !string.IsNullOrEmpty(value) ? value : DefaultDefaultScreen;
    }

    /// <summary>
    /// Lưu màn hình mặc định
    /// </summary>
    public void SaveDefaultScreen(string screen)
    {
        if (string.IsNullOrEmpty(screen)) return;
        SetValue(DefaultScreenKey, screen);
    }

    #endregion

    #region BaseUrl

    /// <summary>
    /// Lấy Base URL
    /// </summary>
    public string GetBaseUrl()
    {
        var value = GetValue(BaseUrlKey, DefaultBaseUrl);
        return !string.IsNullOrEmpty(value) ? value : DefaultBaseUrl;
    }

    /// <summary>
    /// Lưu Base URL vào Settings
    /// </summary>
    public void SaveBaseUrl(string url)
    {
        SetValue(BaseUrlKey, url ?? DefaultBaseUrl);
    }

    /// <summary>
    /// Get full server URL with ensure trailing slash
    /// </summary>
    public string GetFullServerUrl()
    {
        var url = GetBaseUrl();
        
        // Ensure trailing slash
        return url.EndsWith("/") ? url : url + "/";
    }

    #endregion
}

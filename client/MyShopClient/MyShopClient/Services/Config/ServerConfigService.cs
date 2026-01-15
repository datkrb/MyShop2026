using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MyShopClient.Services.Config;

/// <summary>
/// Service để quản lý cấu hình server (URL, port)
/// </summary>
public class ServerConfigService
{
    private const string ServerUrlKey = "ServerUrl";

    /// <summary>
    /// Lấy Server URL từ LocalSettings
    /// </summary>
    public string GetServerUrl()
    {
        try
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var value = localSettings.Values[ServerUrlKey];
            return value as string ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Kiểm tra xem đã có cấu hình server chưa
    /// </summary>
    public bool HasServerConfig()
    {
        return !string.IsNullOrEmpty(GetServerUrl());
    }

    /// <summary>
    /// Lưu Server URL vào LocalSettings
    /// </summary>
    public void SaveServerUrl(string url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            // Đảm bảo URL kết thúc bằng /
            if (!url.EndsWith("/"))
            {
                url += "/";
            }

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[ServerUrlKey] = url;

            // Cập nhật BaseApiService
            Api.BaseApiService.UpdateBaseUrl(url);
        }
        catch
        {
            // Silently fail if settings cannot be saved
        }
    }

    /// <summary>
    /// Test kết nối tới server
    /// </summary>
    /// <param name="url">URL để test</param>
    /// <returns>Tuple gồm success và message</returns>
    public async Task<(bool Success, string Message)> TestConnectionAsync(string url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return (false, "URL không được để trống");
            }

            // Đảm bảo URL kết thúc bằng /
            if (!url.EndsWith("/"))
            {
                url += "/";
            }

            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };

            // Thử gọi tới endpoint health hoặc root
            var response = await client.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                return (true, "Kết nối thành công!");
            }
            else
            {
                return (false, $"Server trả về lỗi: {response.StatusCode}");
            }
        }
        catch (TaskCanceledException)
        {
            return (false, "Kết nối timeout. Vui lòng kiểm tra lại URL.");
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Không thể kết nối: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}");
        }
    }
}

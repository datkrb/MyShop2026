using System;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.Services.Config;
using MyShopClient.Services.Api;
using MyShopClient.ViewModels.Base;

namespace MyShopClient.ViewModels;

public partial class ServerConfigViewModel : ViewModelBase
{
    private readonly AppSettingsService _appSettings;

    [ObservableProperty]
    private string _serverUrl = "http://localhost:3000";

    [ObservableProperty]
    private bool _isTestingConnection;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isStatusSuccess;

    /// <summary>
    /// Full URL for display
    /// </summary>
    public string FullServerUrl => ServerUrl.EndsWith("/") ? ServerUrl : ServerUrl + "/";

    public ServerConfigViewModel(AppSettingsService appSettings)
    {
        _appSettings = appSettings;
        LoadConfig();
    }

    private void LoadConfig()
    {
        ServerUrl = _appSettings.GetBaseUrl();
    }

    partial void OnServerUrlChanged(string value)
    {
        OnPropertyChanged(nameof(FullServerUrl));
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        if (IsTestingConnection) return;

        IsTestingConnection = true;
        StatusMessage = string.Empty;

        try
        {
            var testUrl = FullServerUrl + "health";
            var (success, message) = await TestConnectionToUrlAsync(testUrl);
            IsStatusSuccess = success;
            StatusMessage = message;
        }
        finally
        {
            IsTestingConnection = false;
        }
    }

    private async Task<(bool Success, string Message)> TestConnectionToUrlAsync(string url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return (false, "URL không được để trống");
            }

            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };

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

    [RelayCommand]
    private void Save()
    {
        if (IsSaving) return;

        IsSaving = true;
        StatusMessage = string.Empty;

        try
        {
            if (string.IsNullOrWhiteSpace(ServerUrl))
            {
                StatusMessage = "Vui lòng nhập URL máy chủ.";
                IsStatusSuccess = false;
                return;
            }

            _appSettings.SaveBaseUrl(ServerUrl);

            // Update BaseApiService with new full URL
            BaseApiService.UpdateBaseUrl(FullServerUrl);

            IsStatusSuccess = true;
            StatusMessage = $"Đã lưu cấu hình thành công! URL: {FullServerUrl}";
        }
        catch
        {
            IsStatusSuccess = false;
            StatusMessage = "Không thể lưu cấu hình.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void ClearUrl()
    {
        ServerUrl = "http://localhost:3000";
        StatusMessage = string.Empty;
    }
}

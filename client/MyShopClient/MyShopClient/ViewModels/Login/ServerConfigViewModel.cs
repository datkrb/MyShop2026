using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.Services.Config;
using MyShopClient.ViewModels.Base;

namespace MyShopClient.ViewModels;

public partial class ServerConfigViewModel : ViewModelBase
{
    private readonly ServerConfigService _configService;

    [ObservableProperty]
    private string _serverUrl = string.Empty;

    [ObservableProperty]
    private bool _isTestingConnection;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isStatusSuccess;

    public ServerConfigViewModel(ServerConfigService configService)
    {
        _configService = configService;
        LoadServerUrl();
    }

    private void LoadServerUrl()
    {
        ServerUrl = _configService.GetServerUrl();
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        if (IsTestingConnection) return;

        IsTestingConnection = true;
        StatusMessage = string.Empty;

        try
        {
            var (success, message) = await _configService.TestConnectionAsync(ServerUrl + "health");
            IsStatusSuccess = success;
            StatusMessage = message;
        }
        finally
        {
            IsTestingConnection = false;
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
            if(ServerUrl == string.Empty)
            {
                StatusMessage = "Vui lòng nhập URL máy chủ.";
                IsStatusSuccess = false;
                return;
            }
            _configService.SaveServerUrl(ServerUrl);
            IsStatusSuccess = true;
            StatusMessage = "Đã lưu cấu hình thành công!";
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
        ServerUrl = string.Empty;
        StatusMessage = string.Empty;
    }
}

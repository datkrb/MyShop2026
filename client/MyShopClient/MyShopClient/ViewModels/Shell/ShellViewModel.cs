using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.ViewModels.Base;
using MyShopClient.Services.Api;
using MyShopClient.Services.Auth;

namespace MyShopClient.ViewModels;

public partial class ShellViewModel : ViewModelBase
{
    private readonly AuthApiService _authApiService;
    private readonly CredentialService _credentialService;

    [ObservableProperty]
    private object? _selectedItem;

    [ObservableProperty]
    private bool _isLoggingOut;

    public ShellViewModel(AuthApiService authApiService, CredentialService credentialService)
    {
        _authApiService = authApiService;
        _credentialService = credentialService;
    }

    public bool IsAdmin => App.Current.IsAdmin;

    [RelayCommand]
    public async Task LogoutAsync()
    {
        if (IsLoggingOut) return;

        IsLoggingOut = true;

        try
        {
            // Gọi API logout
            await _authApiService.LogoutAsync();

            // Xóa saved credentials
            _credentialService.ClearCredentials();

            // Clear auth token
            BaseApiService.ClearAuthToken();

            System.Diagnostics.Debug.WriteLine("Logout performed successfully");

            // Navigate về Login screen
            App.Current.RootFrame?.Navigate(typeof(Views.Login.LoginView));
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Logout error: {ex.Message}");
            
            // Vẫn logout locally dù API fail
            _credentialService.ClearCredentials();
            BaseApiService.ClearAuthToken();
            App.Current.RootFrame?.Navigate(typeof(Views.Login.LoginView));
        }
        finally
        {
            IsLoggingOut = false;
        }
    }
}

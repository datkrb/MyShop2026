using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using MyShopClient.ViewModels.Base;
using MyShopClient.Services.Api;
using MyShopClient.Services.Auth;
using System.Threading.Tasks;

namespace MyShopClient.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly AuthApiService _authApiService;
    private readonly CredentialService _credentialService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _rememberMe;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public LoginViewModel(AuthApiService authApiService, CredentialService credentialService)
    {
        _authApiService = authApiService;
        _credentialService = credentialService;
    }

    /// <summary>
    /// Thử auto-login với credentials đã lưu
    /// </summary>
    /// <returns>True nếu login thành công</returns>
    public async Task<bool> TryAutoLoginAsync()
    {
        var credentials = _credentialService.GetCredentials();
        if (credentials == null) return false;

        try
        {
            var response = await _authApiService.LoginAsync(credentials.Value.Username, credentials.Value.Password);
            
            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                System.Diagnostics.Debug.WriteLine("Auto-login successful");
                return true;
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Auto-login failed: {ex.Message}");
            // Xóa credentials không hợp lệ
            _credentialService.ClearCredentials();
        }

        return false;
    }

    [RelayCommand]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter username and password";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var response = await _authApiService.LoginAsync(Username, Password);

            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                // Lưu credentials nếu RememberMe được chọn
                if (RememberMe)
                {
                    _credentialService.SaveCredentials(Username, Password);
                }
                else
                {
                    // Xóa credentials nếu không chọn RememberMe
                    _credentialService.ClearCredentials();
                }

                // Navigate to ShellPage
                App.Current.RootFrame?.Navigate(typeof(Views.ShellPage));
            }
            else
            {
                ErrorMessage = "Invalid credentials";
            }
        }
        catch (System.Exception ex)
        {
            ErrorMessage = $"Login failed: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Signup()
    {
        // TODO: Navigate to Signup
    }

    [RelayCommand]
    private void ForgotPassword()
    {
        // TODO: Navigate to Forgot Password
    }

    [RelayCommand]
    private void SocialLogin(string provider)
    {
        // TODO: Implement Social Login
    }
}

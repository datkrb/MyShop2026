using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using MyShopClient.ViewModels.Base;
using MyShopClient.Services.Api;
using System.Threading.Tasks;

namespace MyShopClient.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly AuthApiService _authApiService;

    [ObservableProperty]
    private string _username = "admin";

    [ObservableProperty]
    private string _password = "123456";

    [ObservableProperty]
    private bool _rememberMe;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public LoginViewModel(AuthApiService authApiService)
    {
        _authApiService = authApiService;
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
                // Save token for future requests
                if (RememberMe)
                {
                    // TODO: Save token to local storage
                    System.Diagnostics.Debug.WriteLine($"Token: {response.Token}");
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

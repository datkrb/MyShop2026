using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using MyShopClient.ViewModels.Base;
using MyShopClient.Services.Api;
using MyShopClient.Services.Auth;
using System.Threading.Tasks;
using System.Reflection;

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

    [ObservableProperty]
    private string _appVersion = "version 1.0.0";

    public LoginViewModel(AuthApiService authApiService, CredentialService credentialService)
    {
        _authApiService = authApiService;
        _credentialService = credentialService;
        
        try
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            AppVersion = $"version {version!.Major}.{version.Minor}.{version.Build}";
        }
        catch
        {
            AppVersion = "version 1.0.0";
        }
    }

    /// <summary>
    /// Thử auto-login với tokens đã lưu
    /// </summary>
    /// <returns>True nếu login thành công</returns>
    public async Task<bool> TryAutoLoginAsync()
    {
        var tokens = _credentialService.GetTokens();
        // Nếu không có token thì không auto login
        if (tokens == null) return false;

        try
        {
            // Set token hiện tại
            _authApiService.SetToken(tokens.Value.AccessToken);

            // Gọi API lấy thông tin user để verify token
            // Nếu token hết hạn, BaseApiService sẽ tự động thử RefreshToken
            var user = await _authApiService.GetCurrentUserAsync();
            
            if (user != null)
            {
                // Store user info
                App.Current.CurrentUserRole = user.Role;
                App.Current.CurrentUserName = user.Username;
                System.Diagnostics.Debug.WriteLine($"Auto-login successful, user: {user.Username}, role: {user.Role}");
                return true;
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Auto-login failed: {ex.Message}");
            // Token không còn hiệu lực (kể cả refresh), xóa đi
            _credentialService.ClearTokens();
            _authApiService.ClearToken();
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

            if (response != null && !string.IsNullOrEmpty(response.AccessToken))
            {
                // Store user info
                App.Current.CurrentUserRole = response.Role;
                App.Current.CurrentUserName = Username;
                System.Diagnostics.Debug.WriteLine($"Login successful, user: {Username}, role: {response.Role}");

                // Lưu tokens nếu RememberMe được chọn
                if (RememberMe && !string.IsNullOrEmpty(response.RefreshToken))
                {
                    _credentialService.SaveTokens(response.AccessToken, response.RefreshToken);
                }
                else
                {
                    // Xóa tokens cũ nếu không chọn RememberMe
                    _credentialService.ClearTokens();
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

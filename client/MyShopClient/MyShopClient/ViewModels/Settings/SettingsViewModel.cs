using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.ViewModels.Base;
using MyShopClient.Services.Config;
using MyShopClient.Services.Navigation;
using MyShopClient.Services.Api;

namespace MyShopClient.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly AppSettingsService _appSettingsService;
    private readonly INavigationService _navigationService;
    private readonly AuthApiService _authApiService;

    // Pagination settings
    [ObservableProperty]
    private int _selectedPageSize = 10;

    [ObservableProperty]
    private ObservableCollection<int> _pageSizeOptions = new() { 5, 10, 15, 20 };

    // Last visited screen settings
    [ObservableProperty]
    private bool _rememberLastScreen = true;

    [ObservableProperty]
    private string _currentLastScreen = "Dashboard";

    // Available screens for display
    [ObservableProperty]
    private ObservableCollection<ScreenOption> _availableScreens = new()
    {
        new ScreenOption { Tag = "Dashboard", DisplayName = "Dashboard", Icon = "\uE74C" },
        new ScreenOption { Tag = "Products", DisplayName = "Products", Icon = "\uE781" },
        new ScreenOption { Tag = "Orders", DisplayName = "Orders", Icon = "\uE7BF" },
        new ScreenOption { Tag = "Customers", DisplayName = "Customers", Icon = "\uE77B" },
        new ScreenOption { Tag = "Settings", DisplayName = "Settings", Icon = "\uE713" }
    };

    // Default start screen
    [ObservableProperty]
    private ScreenOption? _selectedDefaultScreen;

    // Change Password
    [ObservableProperty]
    private string _currentPassword = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private bool _isChangingPassword;

    [ObservableProperty]
    private string _passwordErrorMessage = string.Empty;

    [ObservableProperty]
    private string _passwordSuccessMessage = string.Empty;

    public SettingsViewModel(AppSettingsService appSettingsService, INavigationService navigationService, AuthApiService authApiService)
    {
        _appSettingsService = appSettingsService;
        _navigationService = navigationService;
        _authApiService = authApiService;
        
        // Load saved settings
        SelectedPageSize = _appSettingsService.GetPageSize();
        RememberLastScreen = _appSettingsService.GetRememberLastScreen();
        CurrentLastScreen = _navigationService.GetLastVisitedPage();
        
        // Load default screen
        var savedDefault = _appSettingsService.GetDefaultScreen();
        SelectedDefaultScreen = AvailableScreens.FirstOrDefault(s => s.Tag == savedDefault) ?? AvailableScreens[0];
    }

    // Auto-save when PageSize changes
    partial void OnSelectedPageSizeChanged(int value)
    {
        _appSettingsService.SavePageSize(value);
    }

    // Auto-save when RememberLastScreen changes
    partial void OnRememberLastScreenChanged(bool value)
    {
        _appSettingsService.SaveRememberLastScreen(value);
    }

    // Auto-save when DefaultScreen changes
    partial void OnSelectedDefaultScreenChanged(ScreenOption? value)
    {
        if (value != null)
        {
            _appSettingsService.SaveDefaultScreen(value.Tag);
        }
    }

    [RelayCommand]
    private void SaveSettings()
    {
        _appSettingsService.SavePageSize(SelectedPageSize);
        _appSettingsService.SaveRememberLastScreen(RememberLastScreen);
        if (SelectedDefaultScreen != null)
        {
            _appSettingsService.SaveDefaultScreen(SelectedDefaultScreen.Tag);
        }
    }

    [RelayCommand]
    private void ResetSettings()
    {
        SelectedPageSize = 10;
        RememberLastScreen = true;
        SelectedDefaultScreen = AvailableScreens[0];
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        PasswordErrorMessage = string.Empty;
        PasswordSuccessMessage = string.Empty;

        // Validation
        if (string.IsNullOrWhiteSpace(CurrentPassword))
        {
            PasswordErrorMessage = "Current password is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            PasswordErrorMessage = "New password is required";
            return;
        }

        if (NewPassword.Length < 6)
        {
            PasswordErrorMessage = "New password must be at least 6 characters";
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            PasswordErrorMessage = "New password and confirm password do not match";
            return;
        }

        IsChangingPassword = true;

        try
        {
            var success = await _authApiService.ChangePasswordAsync(CurrentPassword, NewPassword);
            
            if (success)
            {
                PasswordSuccessMessage = "Password changed successfully!";
                CurrentPassword = string.Empty;
                NewPassword = string.Empty;
                ConfirmPassword = string.Empty;
            }
            else
            {
                PasswordErrorMessage = "Failed to change password. Please check your current password.";
            }
        }
        catch (System.Exception ex)
        {
            PasswordErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsChangingPassword = false;
        }
    }
}

public class ScreenOption
{
    public string Tag { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

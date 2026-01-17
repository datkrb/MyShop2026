using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.Services.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;
using MyShopClient.Services.Api;

namespace MyShopClient.ViewModels.Shared;

public partial class PageHeaderViewModel : ObservableObject
{
    private readonly CredentialService _credentialService;
    private readonly AuthApiService _authApiService;

    public string UserName => App.Current.CurrentUserName ?? "User";
    public string UserRole => App.Current.CurrentUserRole ?? "Role";

    public PageHeaderViewModel()
    {
        _credentialService = App.Current.Services.GetService<CredentialService>()!;
        _authApiService = App.Current.Services.GetService<AuthApiService>()!;
    }

    [RelayCommand]
    private async void Logout()
    {
        var dialog = new ContentDialog
        {
            Title = "Confirm Logout",
            Content = "Are you sure you want to logout?",
            PrimaryButtonText = "Logout",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = App.Current.ContentFrame.XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            try 
            {
                // Call API logout
                await _authApiService.LogoutAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logout API failed: {ex.Message}");
            }

            // Clear credentials
            _credentialService?.ClearTokens();
            
            // Clear auth token
            Services.Api.BaseApiService.ClearAuthToken();

            // Clear app state
            App.Current.CurrentUserName = string.Empty;
            App.Current.CurrentUserRole = string.Empty;

            // Navigate to login
            App.Current.RootFrame?.Navigate(typeof(Views.Login.LoginView));
        }
    }
}

using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.Services.Api;
using MyShopClient.Services.Navigation;
using MyShopClient.ViewModels.Base;
using MyShopClient.Views.Login;

namespace MyShopClient.ViewModels;

public partial class ShellViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly AuthApiService _authApiService;

    [ObservableProperty]
    private object? _selectedItem;

    [ObservableProperty]
    private bool _canGoBack;

    public ShellViewModel(INavigationService navigationService, AuthApiService authApiService)
    {
        _navigationService = navigationService;
        _authApiService = authApiService;
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _authApiService.LogoutAsync();
        
        // Clear current user data
        App.Current.CurrentUserRole = string.Empty;
        App.Current.CurrentUserName = string.Empty;
        
        // Navigate to login page (use RootFrame to navigate to login)
        App.Current.RootFrame?.Navigate(typeof(LoginView));
    }

    public bool IsAdmin => App.Current.IsAdmin;

    /// <summary>
    /// Updates the CanGoBack property based on NavigationService state.
    /// Call this after navigation events.
    /// </summary>
    public void UpdateCanGoBack()
    {
        CanGoBack = _navigationService.CanGoBack;
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.GoBack();
        UpdateCanGoBack();
    }
}

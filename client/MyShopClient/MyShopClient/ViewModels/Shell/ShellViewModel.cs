using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.Services.Navigation;
using MyShopClient.ViewModels.Base;

namespace MyShopClient.ViewModels;

public partial class ShellViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private object? _selectedItem;

    [ObservableProperty]
    private bool _canGoBack;

    public ShellViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
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

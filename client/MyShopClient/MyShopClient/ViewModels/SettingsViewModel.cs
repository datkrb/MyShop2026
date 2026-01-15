using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.ViewModels.Base;
using MyShopClient.Services.Config;
using MyShopClient.Services.Navigation;

namespace MyShopClient.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly AppSettingsService _appSettingsService;
    private readonly INavigationService _navigationService;

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

    public SettingsViewModel(AppSettingsService appSettingsService, INavigationService navigationService)
    {
        _appSettingsService = appSettingsService;
        _navigationService = navigationService;
        
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
}

public class ScreenOption
{
    public string Tag { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

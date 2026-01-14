using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.ViewModels.Base;

namespace MyShopClient.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
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
        new ScreenOption { Tag = "Statistics", DisplayName = "Statistics", Icon = "\uE9D2" },
        new ScreenOption { Tag = "Invoices", DisplayName = "Invoices", Icon = "\uE946" }
    };

    // Default start screen
    [ObservableProperty]
    private ScreenOption? _selectedDefaultScreen;

    public SettingsViewModel()
    {
        // Initialize default screen selection
        SelectedDefaultScreen = AvailableScreens[0];
    }

    [RelayCommand]
    private void SaveSettings()
    {
        // TODO: Implement save logic
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

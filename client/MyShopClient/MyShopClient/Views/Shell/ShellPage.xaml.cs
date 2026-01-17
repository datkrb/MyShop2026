using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShopClient.Services.Navigation;
using MyShopClient.Services.Config;
using MyShopClient.ViewModels;

namespace MyShopClient.Views;

public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel { get; }
    private readonly INavigationService _navigationService;
    private readonly AppSettingsService _appSettingsService;

    public ShellPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetService<ShellViewModel>()!;
        _navigationService = App.Current.Services.GetRequiredService<INavigationService>();
        _appSettingsService = App.Current.Services.GetRequiredService<AppSettingsService>();
        App.Current.ContentFrame = ContentFrame;
        
        // Navigate to appropriate page based on settings
        this.Loaded += (s, e) =>
        {
            string startPage;
            
            if (_appSettingsService.GetRememberLastScreen())
            {
                // Use last visited page if RememberLastScreen is ON
                startPage = _navigationService.GetLastVisitedPage();
            }
            else
            {
                // Use default screen if RememberLastScreen is OFF
                startPage = _appSettingsService.GetDefaultScreen();
            }
            
            NavigateToPage(startPage);
            SelectNavItemByTag(startPage);
        };
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer != null)
        {
            var tag = args.SelectedItemContainer.Tag?.ToString();
            NavigateToPage(tag);
        }
    }

    private void NavigateToPage(string? tag)
    {
        if (string.IsNullOrEmpty(tag)) return;

        var pageType = tag switch
        {
            "Dashboard" => typeof(Views.Dashboard.DashboardView),
            "Products" => typeof(Views.Products.ProductsView),
            "Orders" => typeof(Views.Orders.OrdersView),
            "Customers" => typeof(Views.Customers.CustomersView),
            "Reports" => typeof(Views.Reports.ReportPage),
            "Settings" => typeof(Views.Settings.SettingsView),
            "Promotions" => typeof(Views.Promotions.PromotionPage),
            _ => (Type?)null
        };

        if (pageType != null && ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
            
            // Save the current page to local settings
            _navigationService.SaveLastVisitedPage(tag);
        }
    }

    /// <summary>
    /// Selects the NavigationViewItem that matches the given tag.
    /// </summary>
    private void SelectNavItemByTag(string tag)
    {
        foreach (var item in NavView.MenuItems)
        {
            if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == tag)
            {
                NavView.SelectedItem = navItem;
                return;
            }
        }
        
        // Fallback to first item (Dashboard) if tag not found
        if (NavView.MenuItems.Count > 0)
        {
            NavView.SelectedItem = NavView.MenuItems[0];
        }
    }
}

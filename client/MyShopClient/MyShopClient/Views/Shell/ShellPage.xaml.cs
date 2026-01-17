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
        
        // Initialize NavigationService with ContentFrame
        _navigationService.Initialize(ContentFrame);
        
        // Listen to ContentFrame navigation to update CanGoBack in ViewModel
        ContentFrame.Navigated += ContentFrame_Navigated;
        
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

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        // Update CanGoBack in ViewModel when navigation occurs
        ViewModel.UpdateCanGoBack();
        
        // Update selected NavigationViewItem based on current page
        UpdateSelectedNavItem(e.SourcePageType);
    }

    private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        // Use ViewModel command for MVVM pattern
        ViewModel.GoBackCommand.Execute(null);
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
    /// Updates the selected NavigationViewItem based on the current page type.
    /// </summary>
    private void UpdateSelectedNavItem(Type? pageType)
    {
        if (pageType == null) return;

        string? tag = pageType.Name switch
        {
            "DashboardView" => "Dashboard",
            "ProductsView" => "Products",
            "OrdersView" => "Orders",
            "CustomersView" => "Customers",
            "ReportPage" => "Reports",
            "SettingsView" => "Settings",
            "PromotionPage" => "Promotions",
            _ => null
        };

        if (tag != null)
        {
            SelectNavItemByTag(tag);
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
                // Avoid re-triggering SelectionChanged
                if (NavView.SelectedItem != navItem)
                {
                    NavView.SelectedItem = navItem;
                }
                return;
            }
        }
        
        // Fallback to first item (Dashboard) if tag not found
        if (NavView.MenuItems.Count > 0 && NavView.SelectedItem != NavView.MenuItems[0])
        {
            NavView.SelectedItem = NavView.MenuItems[0];
        }
    }
}

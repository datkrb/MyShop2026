using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShopClient.Services.Navigation;
using MyShopClient.ViewModels;

namespace MyShopClient.Views;

public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel { get; }
    private readonly INavigationService _navigationService;

    public ShellPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetService<ShellViewModel>()!;
        _navigationService = App.Current.Services.GetRequiredService<INavigationService>();
        App.Current.ContentFrame = ContentFrame;
        
        // Navigate to last visited page (or Dashboard as default)
        this.Loaded += (s, e) =>
        {
            var lastPage = _navigationService.GetLastVisitedPage();
            NavigateToPage(lastPage);
            SelectNavItemByTag(lastPage);
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

    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer != null)
        {
            var tag = args.InvokedItemContainer.Tag?.ToString();
            
            // Handle Logout separately
            if (tag == "Logout")
            {
                ViewModel.LogoutCommand.Execute(null);
                return;
            }
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
            // "Statistics" => typeof(Views.Statistics.StatisticsView),
            // "Invoices" => typeof(Views.Invoices.InvoicesView),
            "Settings" => typeof(Views.Settings.SettingsView),
            // "Help" => typeof(Views.Help.HelpView),
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

    /// <summary>
    /// Handler for Logout button tap - calls ViewModel LogoutCommand
    /// </summary>
    private void LogoutButton_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        ViewModel.LogoutCommand.Execute(null);
    }
}

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShopClient.ViewModels;

namespace MyShopClient.Views;

public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel { get; }

    public ShellPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetService<ShellViewModel>()!;
        App.Current.ContentFrame = ContentFrame;
        
        // Navigate to Dashboard by default
        this.Loaded += (s, e) =>
        {
            ContentFrame.Navigate(typeof(Views.Dashboard.DashboardView));
            NavView.SelectedItem = NavView.MenuItems[0];
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
                // TODO: Implement logout logic
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
            // "Settings" => typeof(Views.Settings.SettingsView),
            // "Help" => typeof(Views.Help.HelpView),
            _ => (Type?)null
        };

        if (pageType != null && ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }
}

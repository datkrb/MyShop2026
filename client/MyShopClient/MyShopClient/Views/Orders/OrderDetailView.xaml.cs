using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShopClient.ViewModels;
using System;

namespace MyShopClient.Views.Orders;

public sealed partial class OrderDetailView : Page
{
    public OrderDetailViewModel ViewModel { get; }

    public OrderDetailView()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetService<OrderDetailViewModel>() 
            ?? new OrderDetailViewModel();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        // Load order data if passed as parameter
        if (e.Parameter is OrderViewModel order)
        {
            ViewModel.LoadOrder(order);
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        // Navigate back
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private async void EditStatusButton_Click(object sender, RoutedEventArgs e)
    {
        // Open CreateOrderDialog in edit mode with current order data
        var dialog = new CreateOrderDialog
        {
            XamlRoot = this.XamlRoot
        };
        
        // Load order data into dialog for editing
        dialog.ViewModel.LoadFromDetailViewModel(ViewModel);
        
        await dialog.ShowAsync();
    }

    private async void DeleteOrderButton_Click(object sender, RoutedEventArgs e)
    {
        // Show confirmation dialog
        var confirmDialog = new ContentDialog
        {
            Title = "Delete Order",
            Content = $"Are you sure you want to delete order {ViewModel.OrderId}? This action cannot be undone.",
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Secondary,
            XamlRoot = this.XamlRoot
        };

        var result = await confirmDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            // TODO: Call API to delete order
            // For now, just navigate back
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }
}

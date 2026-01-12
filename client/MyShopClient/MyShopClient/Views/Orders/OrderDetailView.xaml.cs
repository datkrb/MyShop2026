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

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is string param && param == "new")
        {
            ViewModel.InitializeNewOrder();
        }
        else if (e.Parameter is OrderViewModel order)
        {
            // Quick load then fetch details
            ViewModel.LoadOrder(order);
        }
        else if (e.Parameter is int orderId)
        {
            // Load directly by ID from API
            await ViewModel.LoadOrderAsync(orderId);
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private void EditStatusButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.EnableEdit();
    }

    private async void DeleteOrderButton_Click(object sender, RoutedEventArgs e)
    {
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
            await ViewModel.DeleteOrderAsync();
        }
    }
}

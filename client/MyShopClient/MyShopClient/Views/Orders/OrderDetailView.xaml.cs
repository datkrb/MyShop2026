using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShopClient.Services.Navigation;
using MyShopClient.ViewModels;
using MyShopClient.Views.Shared;
using System;
using System.Threading.Tasks;

namespace MyShopClient.Views.Orders;

public sealed partial class OrderDetailView : Page
{
    public OrderDetailViewModel ViewModel { get; }
    private readonly INavigationService _navigationService;

    public OrderDetailView()
    {
        this.InitializeComponent();
        _navigationService = App.Current.Services.GetRequiredService<INavigationService>();
        ViewModel = App.Current.Services.GetService<OrderDetailViewModel>() 
            ?? new OrderDetailViewModel(
                App.Current.Services.GetRequiredService<MyShopClient.Services.Api.OrderApiService>(),
                _navigationService);
        
        // Subscribe to notification property changes
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.IsTipOpen) && ViewModel.IsTipOpen)
        {
            ShowNotification();
            ViewModel.IsTipOpen = false; // Reset flag
        }
    }

    private void ShowNotification()
    {
        // Use the SlideNotification component based on severity
        var severity = ViewModel.InfoBarSeverity switch
        {
            "Success" => NotificationSeverity.Success,
            "Warning" => NotificationSeverity.Warning,
            "Error" => NotificationSeverity.Error,
            _ => NotificationSeverity.Info
        };
        
        Notification.Show(ViewModel.InfoBarMessage, severity);
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
        _navigationService.GoBack();
    }
}

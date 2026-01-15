using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShopClient.Services.Navigation;
using MyShopClient.ViewModels;
using MyShopClient.Views.Shared;
using System;

namespace MyShopClient.Views.Customers;

public sealed partial class CustomersView : Page
{
    public CustomersViewModel ViewModel { get; }
    private readonly INavigationService _navigationService;

    public CustomersView()
    {
        this.InitializeComponent();
        _navigationService = App.Current.Services.GetRequiredService<INavigationService>();
        ViewModel = App.Current.Services.GetService<CustomersViewModel>()!;
    }
    
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        // Load customers from API when navigating to this page
        await ViewModel.LoadCustomersAsync();
    }
    
    private void CustomersListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is CustomerViewModel customer)
        {
            _navigationService.Navigate(typeof(CustomerDetailView), customer);
        }
    }
    
    private async void AddCustomerButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddCustomerDialog
        {
            XamlRoot = this.XamlRoot
        };
        
        dialog.ViewModel.Reset();
        
        var result = await dialog.ShowAsync();
        
        if (result == ContentDialogResult.Primary)
        {
            // Show success notification and refresh list
            Notification.ShowSuccess("Khách hàng đã được thêm thành công!");
            await ViewModel.LoadCustomersAsync();
        }
    }
    
    private void OnPageChanged(object sender, int pageNumber)
    {
        _ = ViewModel.GoToPageAsync(pageNumber);
    }
}

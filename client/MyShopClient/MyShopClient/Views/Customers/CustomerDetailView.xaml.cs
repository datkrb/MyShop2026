using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShopClient.Services.Navigation;
using MyShopClient.ViewModels;
using MyShopClient.Views.Shared;
using System;
using System.Threading.Tasks;

namespace MyShopClient.Views.Customers;

public sealed partial class CustomerDetailView : Page
{
    public CustomerDetailViewModel ViewModel { get; }
    private readonly INavigationService _navigationService;

    public CustomerDetailView()
    {
        this.InitializeComponent();
        _navigationService = App.Current.Services.GetRequiredService<INavigationService>();
        ViewModel = App.Current.Services.GetRequiredService<CustomerDetailViewModel>();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        if (e.Parameter is CustomerViewModel customer)
        {
            ViewModel.LoadCustomer(customer);
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        _navigationService.GoBack();
    }

    private void OrderItem_Click(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is CustomerOrderViewModel order)
        {
            ViewModel.NavigateToOrderCommand.Execute(order);
        }
    }

    private async void EditCustomerButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddCustomerDialog
        {
            XamlRoot = this.XamlRoot
        };
        
        dialog.ViewModel.LoadFromDetailViewModel(ViewModel);
        
        var result = await dialog.ShowAsync();
        
        if (result == ContentDialogResult.Primary)
        {
            // Reload customer details after edit
            await ViewModel.LoadCustomerDetailsAsync(ViewModel.Id);
            Notification.ShowSuccess("Thông tin khách hàng đã được cập nhật!");
        }
    }

    private async void DeleteCustomerButton_Click(object sender, RoutedEventArgs e)
    {
        var confirmDialog = new ContentDialog
        {
            Title = "Xóa khách hàng",
            Content = $"Bạn có chắc chắn muốn xóa {ViewModel.Name}? Hành động này không thể hoàn tác.",
            PrimaryButtonText = "Xóa",
            SecondaryButtonText = "Hủy",
            DefaultButton = ContentDialogButton.Secondary,
            XamlRoot = this.XamlRoot
        };

        var result = await confirmDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var success = await ViewModel.DeleteCustomerAsync();
            
            if (success)
            {
                Notification.ShowSuccess("Khách hàng đã được xóa thành công!");
                
                // Wait for user to see notification, then go back
                await Task.Delay(1500);
                _navigationService.GoBack();
            }
            else
            {
                Notification.ShowError("Không thể xóa khách hàng. Vui lòng thử lại.");
            }
        }
    }
}

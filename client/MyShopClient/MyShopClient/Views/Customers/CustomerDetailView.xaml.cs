using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShopClient.ViewModels;
using MyShopClient.Views.Shared;
using System;
using System.Threading.Tasks;

namespace MyShopClient.Views.Customers;

public sealed partial class CustomerDetailView : Page
{
    public CustomerDetailViewModel ViewModel { get; }

    public CustomerDetailView()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetService<CustomerDetailViewModel>() 
            ?? new CustomerDetailViewModel(App.Current.Services.GetRequiredService<MyShopClient.Services.Api.CustomerApiService>());
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
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
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
                
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
            }
            else
            {
                Notification.ShowError("Không thể xóa khách hàng. Vui lòng thử lại.");
            }
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShopClient.ViewModels;
using System;

namespace MyShopClient.Views.Customers;

public sealed partial class CustomerDetailView : Page
{
    public CustomerDetailViewModel ViewModel { get; }

    public CustomerDetailView()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetService<CustomerDetailViewModel>() 
            ?? new CustomerDetailViewModel();
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
        
        await dialog.ShowAsync();
        
        // Reload customer details after edit
        await ViewModel.LoadCustomerDetailsAsync(ViewModel.Id);
    }

    private async void DeleteCustomerButton_Click(object sender, RoutedEventArgs e)
    {
        var confirmDialog = new ContentDialog
        {
            Title = "Delete Customer",
            Content = $"Are you sure you want to delete {ViewModel.Name}? This action cannot be undone.",
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Secondary,
            XamlRoot = this.XamlRoot
        };

        var result = await confirmDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var success = await ViewModel.DeleteCustomerAsync();
            
            if (success && Frame.CanGoBack)
            {
                Frame.GoBack();
            }
            else if (!success)
            {
                // Show error dialog
                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Failed to delete customer. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }
    }
}

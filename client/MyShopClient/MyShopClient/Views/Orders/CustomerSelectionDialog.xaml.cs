using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShopClient.ViewModels;

namespace MyShopClient.Views.Orders;

public sealed partial class CustomerSelectionDialog : ContentDialog
{
    public CustomersViewModel ViewModel { get; }

    public CustomerSelectionDialog()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetService<CustomersViewModel>() 
            ?? new CustomersViewModel();
        ViewModel.IsSelectionMode = true;
    }

    public async void LoadCustomers()
    {
        await ViewModel.LoadCustomersAsync();
    }

    private void PageButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int pageNumber)
        {
            _ = ViewModel.GoToPageAsync(pageNumber);
        }
    }
}

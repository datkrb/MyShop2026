using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MyShopClient.ViewModels;

namespace MyShopClient.Views.Orders;

public sealed partial class CustomerSelectionDialog : ContentDialog
{
    public CustomersViewModel ViewModel { get; }

    public CustomerSelectionDialog()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<CustomersViewModel>();
        ViewModel.IsSelectionMode = true;
    }

    public async void LoadCustomers()
    {
        await ViewModel.LoadCustomersAsync();
    }

    private void OnPageChanged(object sender, int pageNumber)
    {
        _ = ViewModel.GoToPageAsync(pageNumber);
    }
}

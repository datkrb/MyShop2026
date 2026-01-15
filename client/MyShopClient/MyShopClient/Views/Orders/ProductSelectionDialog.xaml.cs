using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShopClient.Services.Api;
using MyShopClient.ViewModels;

namespace MyShopClient.Views.Orders;

public sealed partial class ProductSelectionDialog : ContentDialog
{
    public ProductSelectionViewModel ViewModel { get; }

    public ProductSelectionDialog()
    {
        this.InitializeComponent();
        ViewModel = new ProductSelectionViewModel(App.Current.Services.GetRequiredService<ProductApiService>());
    }

    public async void LoadProducts()
    {
        await ViewModel.LoadProducts();
    }
    
    private void OnPageChanged(object sender, int pageNumber)
    {
        _ = ViewModel.GoToPageAsync(pageNumber);
    }
}

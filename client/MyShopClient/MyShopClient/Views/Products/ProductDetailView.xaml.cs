using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShopClient.ViewModels;

namespace MyShopClient.Views.Products;

public sealed partial class ProductDetailView : Page
{
    public ProductDetailViewModel ViewModel { get; }

    public ProductDetailView()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<ProductDetailViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is int productId)
        {
            await ViewModel.InitializeAsync(productId);
        }
    }
}

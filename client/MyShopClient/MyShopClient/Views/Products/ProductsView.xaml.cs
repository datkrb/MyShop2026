using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MyShopClient.ViewModels;
using MyShopClient.Services.Api;

namespace MyShopClient.Views.Products;

public sealed partial class ProductsView : Page
{
    public ProductViewModel ViewModel { get; }

    public ProductsView()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<ProductViewModel>();
        this.DataContext = ViewModel;
        this.Loaded += ProductsView_Loaded;
    }

    private async void ProductsView_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.LoadDataAsync();
    }

    private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        ViewModel.SearchCommand.Execute(null);
    }

    private void ClearPrice_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.MinPrice = null;
        ViewModel.MaxPrice = null;
        ViewModel.SearchId = null;
        ViewModel.LoadProductsCommand.Execute(null);
    }

    private async void AddCategory_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var dialog = new AddCategoryDialog
        {
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(dialog.CategoryName))
        {
            var newCat = await ProductApiService.Instance.CreateCategoryAsync(dialog.CategoryName, dialog.CategoryDescription);
            if (newCat != null)
            {
                await ViewModel.LoadDataAsync();
            }
        }
    }
}

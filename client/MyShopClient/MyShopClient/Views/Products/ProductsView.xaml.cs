using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MyShopClient.ViewModels;
using MyShopClient.Services.Api;
using MyShopClient.Models;

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

    private async void ProductsView_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadDataAsync();
    }

    private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        ViewModel.SearchCommand.Execute(null);
    }

    private void ProductsListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is ApiProduct product)
        {
            // Navigate to detail view
            App.Current.ContentFrame?.Navigate(typeof(ProductDetailView), product.Id);
        }
    }

    private void AddProductButton_Click(object sender, RoutedEventArgs e)
    {
        // Navigate to add product view (using detail view with ID = 0 for create mode)
        App.Current.ContentFrame?.Navigate(typeof(AddProductView));
    }

    private async void AddCategory_Click(object sender, RoutedEventArgs e)
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
                Notification.ShowSuccess("Category created successfully!");
                await ViewModel.LoadDataAsync();
            }
            else
            {
                Notification.ShowError("Failed to create category.");
            }
        }
    }

    private async void OnPageChanged(object sender, int page)
    {
        await ViewModel.GoToPageAsync(page);
    }
}

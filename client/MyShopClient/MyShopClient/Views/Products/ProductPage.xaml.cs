using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MyShopClient.ViewModels;
using MyShopClient.Services.Api;

namespace MyShopClient.Views.Products;

public sealed partial class ProductPage : Page
{
    public ProductViewModel ViewModel { get; }

    public ProductPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<ProductViewModel>();
        this.DataContext = ViewModel;
        this.Loaded += ProductPage_Loaded;
    }

    private async void ProductPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
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
        var nameBox = new TextBox { Header = "Category Name", PlaceholderText = "Enter name..." };
        var descBox = new TextBox { Header = "Description", PlaceholderText = "Optional...", Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0) };
        
        var stack = new StackPanel();
        stack.Children.Add(nameBox);
        stack.Children.Add(descBox);

        var dialog = new ContentDialog
        {
            XamlRoot = this.XamlRoot,
            Title = "Add New Category",
            Content = stack,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(nameBox.Text))
        {
            var newCat = await ProductApiService.Instance.CreateCategoryAsync(nameBox.Text, descBox.Text);
            if (newCat != null)
            {
                await ViewModel.LoadDataAsync();
            }
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MyShopClient.ViewModels;

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
}

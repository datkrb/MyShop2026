using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
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
        ViewModel = App.Current.Services.GetService<ProductDetailViewModel>() 
            ?? new ProductDetailViewModel();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        // Load product data if passed as parameter
        if (e.Parameter is ProductViewModel product)
        {
            ViewModel.LoadProduct(product);
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        // Navigate back
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }
    
    private void ThumbnailItem_Click(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is ProductImageItem imageItem)
        {
            var index = ViewModel.ProductImages.IndexOf(imageItem);
            if (index >= 0)
            {
                ViewModel.SelectedImageIndex = index;
            }
        }
    }
}

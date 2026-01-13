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
    private void DeleteExistingImage_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int id)
        {
            ViewModel.DeleteExistingImageCommand.Execute(id);
        }
    }

    private void DeleteButton_Loading(Microsoft.UI.Xaml.FrameworkElement sender, object args)
    {
        if (sender is Button button)
        {
            var binding = new Microsoft.UI.Xaml.Data.Binding
            {
                Source = ViewModel,
                Path = new Microsoft.UI.Xaml.PropertyPath("IsEditing"),
                Converter = (Microsoft.UI.Xaml.Data.IValueConverter)Resources["BoolToVisibleConverter"],
                Mode = Microsoft.UI.Xaml.Data.BindingMode.OneWay
            };
            button.SetBinding(Microsoft.UI.Xaml.UIElement.VisibilityProperty, binding);
        }
    }
    private void RemoveSelectedImage_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string path)
        {
            ViewModel.RemoveSelectedImageCommand.Execute(path);
        }
    }
}

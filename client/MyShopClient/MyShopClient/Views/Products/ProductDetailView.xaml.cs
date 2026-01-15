using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShopClient.Services.Navigation;
using MyShopClient.ViewModels;
using MyShopClient.Models;

namespace MyShopClient.Views.Products;

public sealed partial class ProductDetailView : Page
{
    public ProductDetailViewModel ViewModel { get; }
    private readonly INavigationService _navigationService;

    public ProductDetailView()
    {
        this.InitializeComponent();
        _navigationService = App.Current.Services.GetRequiredService<INavigationService>();
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

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        _navigationService.GoBack();
    }

    private void EditProductButton_Click(object sender, RoutedEventArgs e)
    {
        // Navigate to AddProductView with the product ID for editing
        if (ViewModel.Product != null)
        {
            _navigationService.Navigate(typeof(AddProductView), ViewModel.Product.Id);
        }
    }

    private async void DeleteProductButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.DeleteCommand.ExecuteAsync(null);
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.SaveCommand.ExecuteAsync(null);
        Notification.ShowSuccess("Product saved successfully!");
    }

    private void CancelEditButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.CancelEditCommand.Execute(null);
    }

    private void PreviousImage_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.PreviousImageCommand.Execute(null);
    }

    private void NextImage_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.NextImageCommand.Execute(null);
    }

    private void Thumbnail_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is ProductImage image)
        {
            ViewModel.SelectImageCommand.Execute(image);
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

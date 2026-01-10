using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.Models;
using MyShopClient.Services.Api;
using MyShopClient.ViewModels.Base;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MyShopClient.ViewModels;

public partial class ProductDetailViewModel : ViewModelBase
{
    private readonly ProductApiService _productApiService;
    private int _productId;

    [ObservableProperty]
    private ApiProduct _product;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();
    
    [ObservableProperty]
    private Category? _selectedCategory;

    public ProductDetailViewModel(ProductApiService productApiService)
    {
        _productApiService = productApiService;
    }

    public async Task InitializeAsync(int productId)
    {
        _productId = productId;
        await LoadProduct();
        await LoadCategories();
    }

    private async Task LoadCategories() 
    {
         var cats = await _productApiService.GetCategoriesAsync();
         if(cats != null)
         {
             Categories = new ObservableCollection<Category>(cats);
             if (Product?.CategoryId != null)
             {
                 SelectedCategory = Categories.FirstOrDefault(c => c.Id == Product.CategoryId);
             }
         }
    }

    private async Task LoadProduct()
    {
        IsLoading = true;
        try
        {
            var p = await _productApiService.GetProductAsync(_productId);
            if (p != null)
            {
                Product = p;
                if (Categories.Count > 0 && Product.CategoryId != null)
                {
                     SelectedCategory = Categories.FirstOrDefault(c => c.Id == Product.CategoryId);
                }
            }
        }
        catch (Exception ex)
        {
             System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void EnableEdit()
    {
        IsEditing = true;
    }

    [RelayCommand]
    public void CancelEdit()
    {
        IsEditing = false;
        _ = LoadProduct(); // Revert changes
    }

    [RelayCommand]
    public async Task Save()
    {
        if (Product == null) return;
        
        IsLoading = true;
        try
        {
            if (SelectedCategory != null) 
            {
                Product.CategoryId = SelectedCategory.Id;
            }

            var updated = await _productApiService.UpdateProductAsync(_productId, Product);
            if (updated != null)
            {
                Product = updated;
                IsEditing = false;
                
                // Show success toast/dialog
            }
        }
        catch (Exception ex)
        {
             System.Diagnostics.Debug.WriteLine($"Save Error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task Delete()
    {
        ContentDialog deleteDialog = new ContentDialog
        {
            XamlRoot = App.Current.MainWindow.Content.XamlRoot,
            Title = "Xác nhận xóa",
            Content = $"Bạn có chắc muốn xóa sản phẩm {Product.Name}?",
            PrimaryButtonText = "Xóa",
            CloseButtonText = "Hủy",
            DefaultButton = ContentDialogButton.Close
        };

        var result = await deleteDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var success = await _productApiService.DeleteProductAsync(_productId);
            if (success)
            {
                // Go back
                App.Current.RootFrame?.GoBack();
            }
        }
    }
    
    [RelayCommand]
    public void GoBack()
    {
        if (App.Current.RootFrame?.CanGoBack == true)
        {
             App.Current.RootFrame.GoBack();
        }
    }
}

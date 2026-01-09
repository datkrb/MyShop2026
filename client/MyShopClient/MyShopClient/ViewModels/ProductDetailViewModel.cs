using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using MyShopClient.ViewModels.Base;
using System;
using System.Collections.ObjectModel;

namespace MyShopClient.ViewModels;

public partial class ProductDetailViewModel : ViewModelBase
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _sku = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private decimal _importPrice;

    [ObservableProperty]
    private decimal _salePrice;

    [ObservableProperty]
    private int _stock;

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private int _selectedImageIndex;

    public ObservableCollection<ProductImageItem> ProductImages { get; } = new();

    public bool HasMultipleImages => ProductImages.Count > 1;

    public ProductDetailViewModel()
    {
    }

    /// <summary>
    /// Initialize the view model with product data
    /// </summary>
    public void LoadProduct(ProductViewModel product)
    {
        Id = product.Id;
        Sku = product.Sku;
        Name = product.Name;
        Category = product.Category;
        SalePrice = product.Price;
        Stock = product.Stock;
        Status = product.Status;

        // Load images (for now using single image from product, can be expanded for multiple)
        ProductImages.Clear();
        
        // Add the main product image
        ProductImages.Add(new ProductImageItem 
        { 
            ImageUrl = product.ImageUrl,
            IsSelected = true
        });

        // Add some mock additional images for demo
        ProductImages.Add(new ProductImageItem 
        { 
            ImageUrl = "https://via.placeholder.com/400x400/7C5CFC/FFFFFF?text=Image+2"
        });
        ProductImages.Add(new ProductImageItem 
        { 
            ImageUrl = "https://via.placeholder.com/400x400/10B981/FFFFFF?text=Image+3"
        });

        SelectedImageIndex = 0;
        OnPropertyChanged(nameof(HasMultipleImages));
    }

    [RelayCommand]
    private void PreviousImage()
    {
        if (SelectedImageIndex > 0)
        {
            SelectedImageIndex--;
        }
        else
        {
            SelectedImageIndex = ProductImages.Count - 1;
        }
    }

    [RelayCommand]
    private void NextImage()
    {
        if (SelectedImageIndex < ProductImages.Count - 1)
        {
            SelectedImageIndex++;
        }
        else
        {
            SelectedImageIndex = 0;
        }
    }

    [RelayCommand]
    private void SelectImage(int index)
    {
        if (index >= 0 && index < ProductImages.Count)
        {
            SelectedImageIndex = index;
        }
    }

    partial void OnSelectedImageIndexChanged(int value)
    {
        // Update selection state on all images
        for (int i = 0; i < ProductImages.Count; i++)
        {
            ProductImages[i].IsSelected = i == value;
        }
    }

    public string StatusBackground => Status switch
    {
        "Published" => "#DCFCE7",
        "Low Stock" => "#FEF3C7",
        "Out of Stock" => "#FEE2E2",
        "Draft" => "#F3F4F6",
        _ => "#F3F4F6"
    };

    public string StatusForeground => Status switch
    {
        "Published" => "#15803D",
        "Low Stock" => "#B45309",
        "Out of Stock" => "#B91C1C",
        "Draft" => "#4B5563",
        _ => "#4B5563"
    };
}

/// <summary>
/// Represents an image in the product detail carousel
/// </summary>
public partial class ProductImageItem : ObservableObject
{
    [ObservableProperty]
    private string _imageUrl = string.Empty;

    [ObservableProperty]
    private bool _isSelected;
}

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
    private ApiProduct _product = new();

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();
    
    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private ObservableCollection<string> _selectedImagePaths = new();

    public bool HasMinimumImages => (Product?.Images?.Count ?? 0) + SelectedImagePaths.Count >= 3;

    // Image Slideshow Properties
    [ObservableProperty]
    private int _currentImageIndex = 0;

    public string? CurrentImage => Product?.Images?.Count > 0 && CurrentImageIndex < Product.Images.Count 
        ? Product.Images[CurrentImageIndex].Url 
        : null;

    public bool HasMultipleImages => (Product?.Images?.Count ?? 0) > 1;

    public string ImageCounterText => $"{CurrentImageIndex + 1} / {Product?.Images?.Count ?? 0}";

    // Formatted display properties
    public string FormattedImportPrice => Product?.ImportPrice.ToString("N0") + " VND" ?? "0 VND";
    public string FormattedSalePrice => Product?.SalePrice.ToString("N0") + " VND" ?? "0 VND";

    public ProductDetailViewModel(ProductApiService productApiService)
    {
        _productApiService = productApiService;
    }

    [RelayCommand]
    public void NextImage()
    {
        if (Product?.Images?.Count > 0)
        {
            CurrentImageIndex = (CurrentImageIndex + 1) % Product.Images.Count;
            OnPropertyChanged(nameof(CurrentImage));
            OnPropertyChanged(nameof(ImageCounterText));
        }
    }

    [RelayCommand]
    public void PreviousImage()
    {
        if (Product?.Images?.Count > 0)
        {
            CurrentImageIndex = CurrentImageIndex > 0 ? CurrentImageIndex - 1 : Product.Images.Count - 1;
            OnPropertyChanged(nameof(CurrentImage));
            OnPropertyChanged(nameof(ImageCounterText));
        }
    }

    [RelayCommand]
    public void SelectImage(ProductImage image)
    {
        if (Product?.Images != null)
        {
            var index = Product.Images.ToList().FindIndex(i => i.Id == image.Id);
            if (index >= 0)
            {
                CurrentImageIndex = index;
                OnPropertyChanged(nameof(CurrentImage));
                OnPropertyChanged(nameof(ImageCounterText));
            }
        }
    }

    public async Task InitializeAsync(int productId)
    {
        _productId = productId;
        await LoadCategories();

        if (_productId == 0)
        {
            // Create mode
            Product = new ApiProduct();
            IsEditing = true;
        }
        else
        {
            // Edit mode
            await LoadProduct();
        }
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

    [RelayCommand]
    public void EnableEdit()
    {
        IsEditing = true;
    }

    [RelayCommand]
    public void CancelEdit()
    {
        IsEditing = false;
        SelectedImagePaths.Clear(); // Clear selected images
        _ = LoadProduct(); // Revert changes
    }

    [RelayCommand]
    public async Task PickImages()
    {
        var currentCount = (Product?.Images?.Count ?? 0) + SelectedImagePaths.Count;
        if (currentCount >= 10)
        {
             ContentDialog limitDialog = new ContentDialog
             {
                 XamlRoot = App.Current.MainWindow.Content.XamlRoot,
                 Title = "Limit Reached",
                 Content = "You can only add up to 10 images per product.",
                 CloseButtonText = "OK",
                 DefaultButton = ContentDialogButton.Close
             };
             await limitDialog.ShowAsync();
             return;
        }

        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        var window = App.Current.MainWindow;
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hWnd);

        picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");

        var files = await picker.PickMultipleFilesAsync();
        if (files != null)
        {
            int addedCount = 0;
            foreach (var file in files)
            {
                if (currentCount + addedCount >= 10)
                {
                     ContentDialog limitDialog = new ContentDialog
                     {
                         XamlRoot = App.Current.MainWindow.Content.XamlRoot,
                         Title = "Limit Reached",
                         Content = "You have reached the maximum of 10 images. Some images were not added.",
                         CloseButtonText = "OK",
                         DefaultButton = ContentDialogButton.Close
                     };
                     await limitDialog.ShowAsync();
                     break;
                }

                if (!SelectedImagePaths.Contains(file.Path))
                {
                    SelectedImagePaths.Add(file.Path);
                    addedCount++;
                }
            }
            OnPropertyChanged(nameof(HasMinimumImages));
        }
    }

    [RelayCommand]
    public void RemoveSelectedImage(string path)
    {
        SelectedImagePaths.Remove(path);
        OnPropertyChanged(nameof(HasMinimumImages));
    }

    [RelayCommand]
    public async Task DeleteExistingImage(int imageId)
    {
        ContentDialog deleteDialog = new ContentDialog
        {
            XamlRoot = App.Current.MainWindow.Content.XamlRoot,
            Title = "Delete Image",
            Content = "Are you sure you want to delete this image?",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close
        };

        var result = await deleteDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var success = await _productApiService.DeleteProductImageAsync(imageId);
            if (success)
            {
               await LoadProduct();
            }
        }
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

            ApiProduct? result;
            if (_productId == 0)
            {
                result = await _productApiService.CreateProductAsync(Product);
            }
            else
            {
                result = await _productApiService.UpdateProductAsync(_productId, Product);
            }

            if (result != null)
            {
                // Upload images if any
                if (SelectedImagePaths.Count > 0)
                {
                    await _productApiService.UploadProductImagesAsync(result.Id, SelectedImagePaths.ToList());
                    SelectedImagePaths.Clear();
                }

                await LoadProduct(result.Id); // Reload to get new images and reset state
                _productId = result.Id;
                IsEditing = false;
            }
        }
        catch (Exception ex)
        {
             System.Diagnostics.Debug.WriteLine($"Save Error: {ex.Message}");
             
             ContentDialog errorDialog = new ContentDialog
             {
                 XamlRoot = App.Current.MainWindow.Content.XamlRoot,
                 Title = "Save Error",
                 Content = $"An error occurred while saving: {ex.Message}",
                 CloseButtonText = "OK",
                 DefaultButton = ContentDialogButton.Close
             };
             await errorDialog.ShowAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadProduct(int? id = null)
    {
        int targetId = id ?? _productId;
        IsLoading = true;
        try
        {
            var p = await _productApiService.GetProductAsync(targetId);
            if (p != null)
            {
                Product = p;
                CurrentImageIndex = 0; // Reset to first image
                if (Categories.Count > 0 && Product.CategoryId != null)
                {
                    SelectedCategory = Categories.FirstOrDefault(c => c.Id == Product.CategoryId);
                }
                OnPropertyChanged(nameof(HasMinimumImages));
                OnPropertyChanged(nameof(CurrentImage));
                OnPropertyChanged(nameof(HasMultipleImages));
                OnPropertyChanged(nameof(ImageCounterText));
                OnPropertyChanged(nameof(FormattedImportPrice));
                OnPropertyChanged(nameof(FormattedSalePrice));
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
                App.Current.ContentFrame?.GoBack();
            }
        }
    }
    
    [RelayCommand]
    public void GoBack()
    {
        if (App.Current.ContentFrame?.CanGoBack == true)
        {
             App.Current.ContentFrame.GoBack();
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using MyShopClient.Models;
using MyShopClient.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

using MyShopClient.Services.Api;
using System.Collections.Generic;

namespace MyShopClient.ViewModels;

public partial class AddProductViewModel : ObservableValidator
{
    private const string ServerBaseUrl = "http://localhost:3000";
    
    // Edit mode properties
    [ObservableProperty]
    private int _productId;

    [ObservableProperty]
    private bool _isEditMode;

    public string PageTitle => IsEditMode ? "Edit Product" : "Add New Product";
    public string SaveButtonText => IsEditMode ? "Update Product" : "Save Product";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
    [MinLength(2, ErrorMessage = "Tên sản phẩm phải có ít nhất 2 ký tự")]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Mã SKU là bắt buộc")]
    private string _sku = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Giá nhập là bắt buộc")]
    [Range(0, double.MaxValue, ErrorMessage = "Giá nhập phải lớn hơn hoặc bằng 0")]
    private double _importPrice;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Giá bán là bắt buộc")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn 0")]
    private double _salePrice;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc")]
    [Range(0, double.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
    private double _stock;

    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private string _selectedStatus = "Draft";

    [ObservableProperty]
    private bool _isDragOver;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasErrors;

    public ObservableCollection<Category> Categories { get; } = new();

    public ObservableCollection<string> Statuses { get; } = new()
    {
        "Published",
        "Draft",
        "Low Stock",
        "Out of Stock"
    };

    public ObservableCollection<LocalProductImage> ProductImages { get; } = new();

    public bool HasImages => ProductImages.Count > 0;

    private List<int> _originalImageIds = new();

    public event EventHandler<bool>? DialogCloseRequested;

    public AddProductViewModel()
    {
        ProductImages.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasImages));
        _ = LoadCategoriesAsync();
    }
    


    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = await ProductApiService.Instance.GetCategoriesAsync();
            if (categories != null)
            {
                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi khi tải danh mục: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddImagesAsync()
    {
        try
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".webp");

            // Get the current window handle for the picker
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Current.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var files = await picker.PickMultipleFilesAsync();
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    await AddImageFromFileAsync(file);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi khi chọn ảnh: {ex.Message}";
        }
    }

    public async Task AddImageFromFileAsync(StorageFile file)
    {
        try
        {
            var bitmapImage = new BitmapImage();
            using (var stream = await file.OpenReadAsync())
            {
                await bitmapImage.SetSourceAsync(stream);
            }

            var productImage = new LocalProductImage
            {
                FilePath = file.Path,
                FileName = file.Name,
                ImageSource = bitmapImage
            };

            ProductImages.Add(productImage);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi khi tải ảnh {file.Name}: {ex.Message}";
        }
    }

    [RelayCommand]
    private void RemoveImage(LocalProductImage image)
    {
        if (image != null)
        {
            ProductImages.Remove(image);
        }
    }

    /// <summary>
    /// Load product data for editing
    /// </summary>
    public async Task LoadProductAsync(int productId)
    {
        ProductId = productId;
        IsEditMode = true;
        IsSaving = true;
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(SaveButtonText));

        try
        {
            var product = await ProductApiService.Instance.GetProductAsync(productId);
            if (product != null)
            {
                Name = product.Name;
                Sku = product.Sku ?? "";
                Description = product.Description ?? "";
                ImportPrice = (double)product.ImportPrice;
                SalePrice = (double)product.SalePrice;
                Stock = product.Stock;
                
                // Set selected category by finding it in the loaded categories
                if (product.CategoryId != null)
                {
                    SelectedCategory = Categories.FirstOrDefault(c => c.Id == product.CategoryId);
                }

                // Load existing images as display (not editable for now)
                ProductImages.Clear();
                _originalImageIds.Clear();

                if (product.Images != null)
                {
                    foreach (var img in product.Images)
                    {
                        // Keep track of original image IDs
                        _originalImageIds.Add(img.Id);

                        // Convert relative URLs to absolute URLs
                        var imageUrl = img.Url;
                        if (!string.IsNullOrEmpty(imageUrl) && !imageUrl.StartsWith("http"))
                        {
                            // Remove leading slash if present and prepend server base URL
                            imageUrl = ServerBaseUrl + (imageUrl.StartsWith("/") ? imageUrl : "/" + imageUrl);
                        }

                        ProductImages.Add(new LocalProductImage
                        {
                            Id = img.Id,
                            FilePath = img.Url,
                            FileName = img.Url.Split('/').LastOrDefault() ?? "image",
                            ImageUrl = imageUrl
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load product: {ex.Message}";
            HasErrors = true;
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = null;
        ValidateAllProperties();

        if (base.HasErrors)
        {
            HasErrors = true;
            var allErrors = new System.Collections.Generic.List<string>();
            foreach (var error in GetErrors())
            {
                if (error is ValidationResult vr && !string.IsNullOrEmpty(vr.ErrorMessage))
                {
                    allErrors.Add(vr.ErrorMessage);
                }
            }
            ErrorMessage = string.Join(Environment.NewLine, allErrors);
            return;
        }

        if (SelectedCategory == null)
        {
            ErrorMessage = "Vui lòng chọn danh mục sản phẩm";
            HasErrors = true;
            return;
        }

        HasErrors = false;
        IsSaving = true;

        try
        {
            if (IsEditMode)
            {
                // Update existing product
                var existingProduct = await ProductApiService.Instance.GetProductAsync(ProductId);
                if (existingProduct != null)
                {
                    existingProduct.Name = Name;
                    existingProduct.Sku = Sku;
                    existingProduct.Description = Description;
                    existingProduct.ImportPrice = (decimal)ImportPrice;
                    existingProduct.SalePrice = (decimal)SalePrice;
                    existingProduct.Stock = (int)Stock;
                    existingProduct.CategoryId = SelectedCategory.Id;

                    await ProductApiService.Instance.UpdateProductAsync(ProductId, existingProduct);

                    // Upload new images for existing product
                    var imagePaths = ProductImages
                        .Where(img => !string.IsNullOrEmpty(img.FilePath) && string.IsNullOrEmpty(img.ImageUrl))
                        .Select(img => img.FilePath!)
                        .ToList();

                    if (imagePaths.Any())
                    {
                        await ProductApiService.Instance.UploadProductImagesAsync(ProductId, imagePaths);
                    }

                    // Delete removed images
                    var currentImageIds = ProductImages
                        .Where(img => img.Id.HasValue)
                        .Select(img => img.Id!.Value)
                        .ToList();

                    var imagesToDelete = _originalImageIds.Except(currentImageIds).ToList();
                    foreach (var imageId in imagesToDelete)
                    {
                        await ProductApiService.Instance.DeleteProductImageAsync(imageId);
                    }
                }

                DialogCloseRequested?.Invoke(this, true);
            }
            else
            {
                // Create new product
                var newProduct = new ApiProduct
                {
                    Name = Name,
                    Sku = Sku,
                    Description = Description,
                    ImportPrice = (decimal)ImportPrice,
                    SalePrice = (decimal)SalePrice,
                    Stock = (int)Stock,
                    CategoryId = SelectedCategory.Id
                };

                var createdProduct = await ProductApiService.Instance.CreateProductAsync(newProduct);
                
                if (createdProduct != null)
                {
                    // Upload images for the new product
                    var imagePaths = ProductImages
                        .Where(img => !string.IsNullOrEmpty(img.FilePath) && string.IsNullOrEmpty(img.ImageUrl))
                        .Select(img => img.FilePath!)
                        .ToList();

                    if (imagePaths.Any())
                    {
                        await ProductApiService.Instance.UploadProductImagesAsync(createdProduct.Id, imagePaths);
                    }
                }

                DialogCloseRequested?.Invoke(this, true);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi khi lưu sản phẩm: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogCloseRequested?.Invoke(this, false);
    }

    partial void OnNameChanged(string value)
    {
        ValidateProperty(value, nameof(Name));
        UpdateHasErrors();
    }

    partial void OnSkuChanged(string value)
    {
        ValidateProperty(value, nameof(Sku));
        UpdateHasErrors();
    }

    partial void OnImportPriceChanged(double value)
    {
        ValidateProperty(value, nameof(ImportPrice));
        UpdateHasErrors();
    }

    partial void OnSalePriceChanged(double value)
    {
        ValidateProperty(value, nameof(SalePrice));
        UpdateHasErrors();
    }

    partial void OnStockChanged(double value)
    {
        ValidateProperty(value, nameof(Stock));
        UpdateHasErrors();
    }

    private void UpdateHasErrors()
    {
        HasErrors = base.HasErrors;
    }

    public string? GetFirstError(string propertyName)
    {
        foreach (var error in GetErrors(propertyName))
        {
            if (error is ValidationResult vr && !string.IsNullOrEmpty(vr.ErrorMessage))
            {
                return vr.ErrorMessage;
            }
        }
        return null;
    }
}

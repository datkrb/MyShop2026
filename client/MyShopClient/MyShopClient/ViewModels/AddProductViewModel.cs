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

namespace MyShopClient.ViewModels;

public partial class AddProductViewModel : ObservableValidator
{
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
    private string? _selectedCategory;

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

    public ObservableCollection<string> Categories { get; } = new()
    {
        "Electronics",
        "Footwear",
        "Accessories",
        "Clothing",
        "Home & Garden"
    };

    public ObservableCollection<string> Statuses { get; } = new()
    {
        "Published",
        "Draft",
        "Low Stock",
        "Out of Stock"
    };

    public ObservableCollection<LocalProductImage> ProductImages { get; } = new();

    public bool HasImages => ProductImages.Count > 0;

    public event EventHandler<bool>? DialogCloseRequested;

    public AddProductViewModel()
    {
        ProductImages.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasImages));
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
                SelectedCategory = product.Category?.Name;

                // Load existing images as display (not editable for now)
                ProductImages.Clear();
                if (product.Images != null)
                {
                    foreach (var img in product.Images)
                    {
                        ProductImages.Add(new LocalProductImage
                        {
                            FilePath = img.Url,
                            FileName = img.Url.Split('/').LastOrDefault() ?? "image",
                            ImageUrl = img.Url
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

        if (string.IsNullOrWhiteSpace(SelectedCategory))
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

                    // Find category ID by name
                    var categories = await ProductApiService.Instance.GetCategoriesAsync();
                    var category = categories?.FirstOrDefault(c => c.Name == SelectedCategory);
                    if (category != null)
                    {
                        existingProduct.CategoryId = category.Id;
                    }

                    await ProductApiService.Instance.UpdateProductAsync(ProductId, existingProduct);
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
                    Stock = (int)Stock
                };

                // Find category ID by name
                var categories = await ProductApiService.Instance.GetCategoriesAsync();
                var category = categories?.FirstOrDefault(c => c.Name == SelectedCategory);
                if (category != null)
                {
                    newProduct.CategoryId = category.Id;
                }

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

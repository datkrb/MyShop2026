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

using Windows.Storage.Pickers;

using MyShopClient.Services.Api;
using System.Collections.Generic;

using MyShopClient.Services.Local;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
    
    /// <summary>
    /// Event fired when a notification should be shown. Args: (message, isError)
    /// </summary>
    public event Action<string, bool>? NotificationRequested;

    private readonly ProductApiService _productApiService;
    private readonly ILocalDraftService _localDraftService;
    private readonly DispatcherTimer _autoSaveTimer;

    public AddProductViewModel(ProductApiService productApiService, ILocalDraftService localDraftService)
    {
        _productApiService = productApiService ?? throw new ArgumentNullException(nameof(productApiService));
        _localDraftService = localDraftService ?? throw new ArgumentNullException(nameof(localDraftService));
        
        ProductImages.CollectionChanged += (s, e) => 
        {
            System.Diagnostics.Debug.WriteLine($"ProductImages.CollectionChanged fired! Count: {ProductImages.Count}, Action: {e.Action}");
            OnPropertyChanged(nameof(HasImages));
            TriggerAutoSave();
        };
        _ = LoadCategoriesAsync();
        
        _autoSaveTimer = new DispatcherTimer();
        _autoSaveTimer.Interval = TimeSpan.FromMilliseconds(500);
        _autoSaveTimer.Tick += AutoSaveTimer_Tick;
        
        this.PropertyChanged += AddProductViewModel_PropertyChanged;
    }

    private void AddProductViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Ignored properties that shouldn't trigger auto-save
        if (e.PropertyName == nameof(IsSaving) || 
            e.PropertyName == nameof(HasImages) || 
            e.PropertyName == nameof(ErrorMessage) ||
            e.PropertyName == nameof(HasErrors) ||
            e.PropertyName == nameof(PageTitle) ||
            e.PropertyName == nameof(SaveButtonText))
        {
            return;
        }

        TriggerAutoSave();
    }
    


    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = await _productApiService.GetCategoriesAsync();
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

    public async Task AddImageFromFileAsync(Windows.Storage.StorageFile file)
    {
        try
        {
            // Get drafts folder path using System.IO
            var draftsFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MyShopClient",
                "Drafts"
            );
            
            // Ensure folder exists
            if (!Directory.Exists(draftsFolder))
            {
                Directory.CreateDirectory(draftsFolder);
            }
            
            string targetFilePath = file.Path;
            
            // If we are in Create mode (Draft mode), copy to Drafts folder
            if (!IsEditMode)
            {
                // Only copy if the file is not already in the Drafts folder
                if (!file.Path.StartsWith(draftsFolder, StringComparison.OrdinalIgnoreCase))
                {
                    var newFileName = Guid.NewGuid() + Path.GetExtension(file.Name);
                    targetFilePath = Path.Combine(draftsFolder, newFileName);
                    
                    // Copy file using System.IO
                    using (var sourceStream = await file.OpenStreamForReadAsync())
                    using (var destStream = File.Create(targetFilePath))
                    {
                        await sourceStream.CopyToAsync(destStream);
                    }
                }
            }

            var bitmapImage = new BitmapImage();
            using (var stream = File.OpenRead(targetFilePath))
            {
                var memStream = new MemoryStream();
                await stream.CopyToAsync(memStream);
                memStream.Position = 0;
                await bitmapImage.SetSourceAsync(memStream.AsRandomAccessStream());
            }

            var productImage = new LocalProductImage
            {
                FilePath = targetFilePath,
                FileName = Path.GetFileName(targetFilePath),
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
            var product = await _productApiService.GetProductAsync(productId);
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
            var allErrors = new System.Collections.Generic.List<string>();
            foreach (var error in GetErrors())
            {
                if (error is ValidationResult vr && !string.IsNullOrEmpty(vr.ErrorMessage))
                {
                    allErrors.Add(vr.ErrorMessage);
                }
            }
            NotificationRequested?.Invoke(string.Join(", ", allErrors), true);
            return;
        }

        if (SelectedCategory == null)
        {
            NotificationRequested?.Invoke("Vui lòng chọn danh mục sản phẩm", true);
            return;
        }

        IsSaving = true;

        try
        {
            if (IsEditMode)
            {
                // Update existing product
                var existingProduct = await _productApiService.GetProductAsync(ProductId);
                if (existingProduct != null)
                {
                    existingProduct.Name = Name;
                    existingProduct.Sku = Sku;
                    existingProduct.Description = Description;
                    existingProduct.ImportPrice = (decimal)ImportPrice;
                    existingProduct.SalePrice = (decimal)SalePrice;
                    existingProduct.Stock = (int)Stock;
                    existingProduct.CategoryId = SelectedCategory.Id;

                    await _productApiService.UpdateProductAsync(ProductId, existingProduct);

                    // Upload new images for existing product
                    var imagePaths = ProductImages
                        .Where(img => !string.IsNullOrEmpty(img.FilePath) && string.IsNullOrEmpty(img.ImageUrl))
                        .Select(img => img.FilePath!)
                        .ToList();

                    if (imagePaths.Any())
                    {
                        await _productApiService.UploadProductImagesAsync(ProductId, imagePaths);
                    }

                    // Delete removed images
                    var currentImageIds = ProductImages
                        .Where(img => img.Id.HasValue)
                        .Select(img => img.Id!.Value)
                        .ToList();

                    var imagesToDelete = _originalImageIds.Except(currentImageIds).ToList();
                    foreach (var imageId in imagesToDelete)
                    {
                        await _productApiService.DeleteProductImageAsync(imageId);
                    }
                }

                DialogCloseRequested?.Invoke(this, true);
                
                // Clear draft on success
                await _localDraftService.ClearProductDraftAsync();
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

                var createdProduct = await _productApiService.CreateProductAsync(newProduct);
                
                if (createdProduct != null)
                {
                    // Upload images for the new product
                    var imagePaths = ProductImages
                        .Where(img => !string.IsNullOrEmpty(img.FilePath) && string.IsNullOrEmpty(img.ImageUrl))
                        .Select(img => img.FilePath!)
                        .ToList();

                    if (imagePaths.Any())
                    {
                        await _productApiService.UploadProductImagesAsync(createdProduct.Id, imagePaths);
                    }
                }

                DialogCloseRequested?.Invoke(this, true);
                
                // Clear draft on success
                await _localDraftService.ClearProductDraftAsync();
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

    public async Task CheckForDraftAsync()
    {
        if (IsEditMode) return;
        
        try
        {
            var draft = await _localDraftService.GetProductDraftAsync();
            // Check if draft has ANY data (any field has value)
            bool hasDraftData = draft != null && 
                (!string.IsNullOrEmpty(draft.Name) || 
                 !string.IsNullOrEmpty(draft.Sku) || 
                 !string.IsNullOrEmpty(draft.Description) ||
                 draft.ImportPrice > 0 ||
                 draft.SalePrice > 0 ||
                 draft.Stock > 0 ||
                 draft.CategoryId > 0 ||
                 (draft.Images != null && draft.Images.Count > 0));
            
            if (hasDraftData)
            {
                 var dialog = new ContentDialog
                 {
                     XamlRoot = App.Current.MainWindow.Content.XamlRoot,
                     Title = "Sản phẩm đang soạn dở",
                     Content = "Bạn có một sản phẩm đang soạn dở. Bạn có muốn tiếp tục không?",
                     PrimaryButtonText = "Tiếp tục",
                     SecondaryButtonText = "Tạo mới (Xóa cũ)",
                     DefaultButton = ContentDialogButton.Primary
                 };

                 var result = await dialog.ShowAsync();

                 if (result == ContentDialogResult.Primary)
                 {
                     Name = draft.Name;
                     Sku = draft.Sku;
                     Description = draft.Description;
                     ImportPrice = (double)draft.ImportPrice;
                     SalePrice = (double)draft.SalePrice;
                     Stock = draft.Stock;
                     // Category logic might need waiting for categories to load
                     if (draft.CategoryId > 0)
                     {
                         // Wait a bit or check if categories are loaded
                         if (Categories.Any())
                             SelectedCategory = Categories.FirstOrDefault(c => c.Id == draft.CategoryId);
                         else
                         {
                             // Categories loading is async, might race. 
                             // We'll set a temporary handler or just rely on re-binding if possible.
                             // For now, simpler:
                             _productApiService.GetCategoriesAsync().ContinueWith(task => 
                             {
                                 if (task.Result != null)
                                 {
                                     App.Current.MainWindow.DispatcherQueue.TryEnqueue(() => 
                                     {
                                         var cats = task.Result;
                                         var match = cats.FirstOrDefault(c => c.Id == draft.CategoryId);
                                         if (match != null)
                                         {
                                             // Find in ObservableCollection
                                             SelectedCategory = Categories.FirstOrDefault(c => c.Id == match.Id);
                                         }
                                     });
                                 }
                             });
                         }
                     }
                     
                     // Restore images if present
                     System.Diagnostics.Debug.WriteLine($"Found {draft.Images?.Count ?? 0} images in draft");
                     if (draft.Images != null && draft.Images.Count > 0)
                     {
                          ProductImages.Clear();
                          foreach (var path in draft.Images)
                          {
                              try 
                              {
                                  System.Diagnostics.Debug.WriteLine($"Restoring image from: {path}");
                                  if (File.Exists(path))
                                  {
                                      var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(path);
                                      await AddImageFromFileAsync(file);
                                      System.Diagnostics.Debug.WriteLine($"Restored image successfully");
                                  }
                                  else
                                  {
                                      System.Diagnostics.Debug.WriteLine($"Image file not found: {path}");
                                  }
                              }
                              catch (Exception ex)
                              { 
                                  System.Diagnostics.Debug.WriteLine($"Failed to restore image {path}: {ex.Message}");
                              }
                          }
                     }
                 }
                 else
                 {
                     await _localDraftService.ClearProductDraftAsync();
                 }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking draft: {ex.Message}");
        }
    }

    private void TriggerAutoSave()
    {
        System.Diagnostics.Debug.WriteLine($"TriggerAutoSave called. IsEditMode: {IsEditMode}");
        if (IsEditMode) return; 
        
        _autoSaveTimer.Stop();
        _autoSaveTimer.Start();
        System.Diagnostics.Debug.WriteLine("AutoSaveTimer started");
    }
    
    private async void AutoSaveTimer_Tick(object sender, object e)
    {
        _autoSaveTimer.Stop();
        await PerformAutoSaveAsync();
    }
    
    private async Task PerformAutoSaveAsync()
    {
        if (IsEditMode) return;
        
        try
        {
            var draft = new ProductDraft
            {
                Name = Name,
                Sku = Sku,
                Description = Description,
                ImportPrice = (decimal)ImportPrice,
                SalePrice = (decimal)SalePrice,
                CategoryId = SelectedCategory?.Id ?? 0,
                Stock = (int)Stock,
                Images = ProductImages.Select(i => i.FilePath ?? "").ToList()
            };
            
            await _localDraftService.SaveProductDraftAsync(draft);
            System.Diagnostics.Debug.WriteLine($"Saved product draft: {draft.Name}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Auto-save product failed: {ex.Message}");
        }
    }
}

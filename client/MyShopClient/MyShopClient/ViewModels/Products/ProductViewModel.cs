using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.Models;
using MyShopClient.Services.Api;
using MyShopClient.Services.Config;
using MyShopClient.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using System;

namespace MyShopClient.ViewModels;

public partial class ProductViewModel : ViewModelBase
{

    [ObservableProperty]
    private ObservableCollection<ApiProduct> _products = new();

    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();

    [ObservableProperty]
    private Category? _selectedCategory;

    // Statistics Properties
    [ObservableProperty]
    private int _statsTotalProducts = 0;

    [ObservableProperty]
    private int _statsTotalCategories = 0;

    [ObservableProperty]
    private int _statsLowStock = 0;

    [ObservableProperty]
    private int _statsOutOfStock = 0;

    // Filter Trigger
    async partial void OnSelectedCategoryChanged(Category? value)
    {
        _currentPage = 1;
        await LoadProducts();
    }

    [ObservableProperty]
    private string _searchKeyword = string.Empty;

    [ObservableProperty]
    private double? _minPrice;

    [ObservableProperty]
    private double? _maxPrice;

    [ObservableProperty]
    private int? _searchId;

    // Advanced Search Properties
    [ObservableProperty]
    private string _stockStatus = "all";

    [ObservableProperty]
    private DateTimeOffset? _createdFrom;

    [ObservableProperty]
    private DateTimeOffset? _createdTo;

    [ObservableProperty]
    private string _skuSearch = string.Empty;

    [ObservableProperty]
    private string _skuMode = "contains";

    public List<string> StockStatusOptions { get; } = new List<string>
    {
        "all",
        "inStock",
        "lowStock",
        "outOfStock"
    };

    public List<string> SkuModeOptions { get; } = new List<string>
    {
        "contains",
        "prefix",
        "exact"
    };

    // Sort Configuration
    public List<string> SortOptions { get; } = new List<string>
    {
        "Newest",
        "Oldest",
        "Price: Low to High",
        "Price: High to Low",
        "Name: A - Z"
    };

    [ObservableProperty]
    private string _selectedSortOption = "Newest";

    partial void OnSelectedSortOptionChanged(string value)
    {
        _currentPage = 1;
        _ = LoadProducts();
    }

    // Paging
    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    partial void OnPageSizeChanged(int value)
    {
        _currentPage = 1;
        _ = LoadProducts();
    }

    [ObservableProperty]
    private int _totalPages = 0;

    [ObservableProperty]
    private int _totalItems = 0;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _canGoNext;

    [ObservableProperty]
    private bool _canGoPrev;

    private readonly Services.Import.ImportService _importService;
    private readonly ProductApiService _productApiService;
    private readonly AppSettingsService _appSettingsService;
    
    public ProductViewModel(ProductApiService productApiService, Services.Import.ImportService importService, AppSettingsService appSettingsService)
    {
        _productApiService = productApiService;
        _importService = importService;
        _appSettingsService = appSettingsService;
        
        // Load PageSize from settings
        _pageSize = _appSettingsService.GetPageSize();
    }

    [RelayCommand]
    public async Task ImportProducts()
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        
        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var window = App.Current.MainWindow;
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        
        // Initialize the file picker with the window handle (HWND).
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hWnd);

        picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
        picker.FileTypeFilter.Add(".xlsx");
        picker.FileTypeFilter.Add(".accdb"); 
        picker.FileTypeFilter.Add(".mdb");

        Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            IsLoading = true;
            try
            {
                List<ApiProduct> imported = new List<ApiProduct>();
                if (file.FileType == ".xlsx")
                {
                    imported = await _importService.ImportFromExcelAsync(file.Path);
                }
                else if (file.FileType == ".accdb" || file.FileType == ".mdb")
                {
                    imported = await _importService.ImportFromAccessAsync(file.Path);
                }

                int successCount = 0;
                foreach(var p in imported)
                {
                    var created = await _productApiService.CreateProductAsync(p);
                    if (created != null) successCount++;
                }

                if (successCount > 0)
                {
                    await LoadProducts();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Import Error: {ex.Message}");
                // Show error dialog
                ContentDialog errorDialog = new ContentDialog
                {
                    XamlRoot = App.Current.MainWindow.Content.XamlRoot,
                    Title = "Import Failed",
                    Content = $"Could not import products: {ex.Message}",
                    CloseButtonText = "OK"
                };
                await errorDialog.ShowAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    public async Task LoadDataAsync()
    {
        IsLoading = true;
        await LoadCategories();
        await LoadProducts();
        IsLoading = false;
    }

    private async Task LoadCategories()
    {
        var cats = await _productApiService.GetCategoriesAsync();
        Categories.Clear();
        // Add "All" option if needed, but UI can handle null selection as "All"
        // Let's add a dummy "All" category or handle null in UI
        // Ideally null works, but WinUI ComboBox is finicky.
        // Let's just create a list with "All" + API cats
        
        var list = new List<Category> { new Category { Id = -1, Name = "All Categories" } };
        if (cats != null)
        {
            list.AddRange(cats);
        }
        
        Categories = new ObservableCollection<Category>(list);
        SelectedCategory = list.First(); 
    }

    private string GetBackendSort(string uiSort)
    {
        return uiSort switch
        {
            "Newest" => "id,desc",
            "Oldest" => "id,asc",
            "Price: Low to High" => "salePrice,asc",
            "Price: High to Low" => "salePrice,desc",
            "Name: A - Z" => "name,asc",
            _ => "id,desc"
        };
    }

    [ObservableProperty]
    private ObservableCollection<PageInfo> _pages = new();

    // PageNumbers for PaginationControl compatibility
    public ObservableCollection<PageButtonModel> PageNumbers { get; } = new();

    [RelayCommand]
    public async Task LoadProducts()
    {
        IsLoading = true;
        try 
        {
            int? catId = (SelectedCategory == null || SelectedCategory.Id == -1) ? null : SelectedCategory.Id;
            string sort = GetBackendSort(SelectedSortOption);

            decimal? minVal = MinPrice.HasValue ? (decimal?)MinPrice.Value : null;
            decimal? maxVal = MaxPrice.HasValue ? (decimal?)MaxPrice.Value : null;

            // Advanced search params
            string? createdFromStr = CreatedFrom?.ToString("yyyy-MM-dd");
            string? createdToStr = CreatedTo?.ToString("yyyy-MM-dd");

            // Load Products with advanced filters
            var result = await _productApiService.GetProductsAsync(
                CurrentPage, PageSize, catId, SearchKeyword, sort, minVal, maxVal, SearchId,
                StockStatus, createdFromStr, createdToStr, null, 
                string.IsNullOrEmpty(SkuSearch) ? null : SkuSearch, 
                string.IsNullOrEmpty(SkuSearch) ? null : SkuMode);
            
            if (result != null)
            {
                Products = new ObservableCollection<ApiProduct>(result.Data);
                TotalItems = result.Total;
                TotalPages = result.TotalPages;
                CurrentPage = result.Page;

                UpdatePagingState();
                GeneratePagination();
            }

            // Load Stats (Run safely so it doesn't block if fails)
            try 
            {
                var stats = await _productApiService.GetProductStatsAsync();
                if (stats != null)
                {
                    StatsTotalProducts = stats.TotalProducts;
                    StatsTotalCategories = stats.TotalCategories;
                    StatsLowStock = stats.LowStock;
                    StatsOutOfStock = stats.OutOfStock;
                }
            }
            catch (Exception ex)
            {
                 System.Diagnostics.Debug.WriteLine($"Error loading stats: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading products: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdatePagingState()
    {
        CanGoPrev = CurrentPage > 1;
        CanGoNext = CurrentPage < TotalPages;
    }

    private void GeneratePagination()
    {
        var list = new ObservableCollection<PageInfo>();
        PageNumbers.Clear();
        
        if (TotalPages <= 1) 
        {
            if (TotalPages == 1)
            {
                list.Add(CreatePageInfo(1));
                PageNumbers.Add(new PageButtonModel { PageNumber = 1, IsCurrentPage = true });
            }
            Pages = list;
            return;
        }

        // Always show page 1
        list.Add(CreatePageInfo(1));
        PageNumbers.Add(new PageButtonModel { PageNumber = 1, IsCurrentPage = CurrentPage == 1 });

        int startPage = Math.Max(2, CurrentPage - 1);
        int endPage = Math.Min(TotalPages - 1, CurrentPage + 1);

        // Add ellipsis after page 1 if needed
        if (startPage > 2)
        {
            list.Add(new PageInfo { Text = "...", IsEnabled = false });
            PageNumbers.Add(new PageButtonModel { IsEllipsis = true });
        }

        // Add middle pages
        for (int i = startPage; i <= endPage; i++)
        {
            list.Add(CreatePageInfo(i));
            PageNumbers.Add(new PageButtonModel { PageNumber = i, IsCurrentPage = CurrentPage == i });
        }

        // Add ellipsis before last page if needed
        if (endPage < TotalPages - 1)
        {
            list.Add(new PageInfo { Text = "...", IsEnabled = false });
            PageNumbers.Add(new PageButtonModel { IsEllipsis = true });
        }

        // Always show last page if there's more than 1 page
        if (TotalPages > 1)
        {
            list.Add(CreatePageInfo(TotalPages));
            PageNumbers.Add(new PageButtonModel { PageNumber = TotalPages, IsCurrentPage = CurrentPage == TotalPages });
        }

        Pages = list;
    }

    private PageInfo CreatePageInfo(int page)
    {
        return new PageInfo 
        { 
            Text = page.ToString(), 
            PageNumber = page, 
            IsCurrent = page == CurrentPage,
            IsEnabled = true,
            Command = new RelayCommand(async () => await GoToPage(page))
        };
    }

    private async Task GoToPage(int page)
    {
        if (page == CurrentPage) return;
        CurrentPage = page;
        await LoadProducts();
    }

    // Public method for PaginationControl compatibility
    public async Task GoToPageAsync(int page)
    {
        await GoToPage(page);
    }

    [RelayCommand]
    public async Task Search()
    {
        CurrentPage = 1;
        await LoadProducts();
    }

    [RelayCommand]
    public void ClearFilters()
    {
        SearchKeyword = string.Empty;
        SelectedCategory = Categories.FirstOrDefault();
        MinPrice = null;
        MaxPrice = null;
        StockStatus = "all";
        CreatedFrom = null;
        CreatedTo = null;
        SkuSearch = string.Empty;
        SkuMode = "contains";
        SelectedSortOption = "Newest";
        CurrentPage = 1;
        _ = LoadProducts();
    }

    [RelayCommand]
    public async Task NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadProducts();
        }
    }

    [RelayCommand]
    public async Task PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadProducts();
        }
    }

    [RelayCommand]
    public async Task DeleteProduct(ApiProduct product)
    {
        if (product == null) return;

        ContentDialog deleteDialog = new ContentDialog
        {
            XamlRoot = App.Current.MainWindow.Content.XamlRoot,
            Title = "Xác nhận xóa",
            Content = $"Bạn có chắc muốn xóa sản phẩm {product.Name}?",
            PrimaryButtonText = "Xóa",
            CloseButtonText = "Hủy",
            DefaultButton = ContentDialogButton.Close
        };

        var result = await deleteDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var success = await _productApiService.DeleteProductAsync(product.Id);
            if (success)
            {
                await LoadProducts();
            }
        }
    }

    [RelayCommand]
    public void AddProduct()
    {
        App.Current.ContentFrame?.Navigate(typeof(Views.Products.ProductDetailView), 0);
    }

    [RelayCommand]
    public void ViewDetail(ApiProduct product)
    {
        App.Current.ContentFrame?.Navigate(typeof(Views.Products.ProductDetailView), product.Id);
    }
}

public class PageInfo
{
    public string Text { get; set; }
    public int PageNumber { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsEnabled { get; set; }
    public System.Windows.Input.ICommand Command { get; set; }
    
    // Helper for binding
    public Microsoft.UI.Xaml.Media.SolidColorBrush Background => IsCurrent 
        ? new Microsoft.UI.Xaml.Media.SolidColorBrush((Windows.UI.Color)Microsoft.UI.Xaml.Application.Current.Resources["SystemAccentColor"])
        : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
        
    public Microsoft.UI.Xaml.Media.SolidColorBrush Foreground => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White);
    public Microsoft.UI.Xaml.Thickness BorderThickness => IsCurrent ? new Microsoft.UI.Xaml.Thickness(0) : new Microsoft.UI.Xaml.Thickness(1);
}

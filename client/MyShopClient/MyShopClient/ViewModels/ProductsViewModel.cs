using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.Models;
using MyShopClient.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShopClient.ViewModels;

public partial class ProductsViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string? _selectedCategory;

    [ObservableProperty]
    private string _minPrice = string.Empty;

    [ObservableProperty]
    private string _maxPrice = string.Empty;

    [ObservableProperty]
    private string? _selectedSortOption = "Newest Added";

    [ObservableProperty]
    private bool _selectAll;

    [ObservableProperty]
    private int _totalProducts;

    [ObservableProperty]
    private int _totalCategories;

    [ObservableProperty]
    private int _lowStockCount;

    [ObservableProperty]
    private int _outOfStockCount;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 1;

    [ObservableProperty]
    private int _totalProductsCount;

    public int TotalPages => TotalProductsCount > 0 ? (int)Math.Ceiling((double)TotalProductsCount / PageSize) : 1;
    public int PageStart => TotalProductsCount > 0 ? (CurrentPage - 1) * PageSize + 1 : 0;
    public int PageEnd => Math.Min(CurrentPage * PageSize, TotalProductsCount);
    public bool CanGoPrevious => CurrentPage > 1;
    public bool CanGoNext => CurrentPage < TotalPages;

    public ObservableCollection<string> Categories { get; } = new();
    public ObservableCollection<ProductViewModel> FilteredProducts { get; } = new();
    public ObservableCollection<PageButtonModel> PageNumbers { get; } = new();
    
    private List<ProductViewModel> _allProducts = new();
    private List<ProductViewModel> _filteredList = new();

    public ProductsViewModel()
    {
        LoadMockData();
        UpdateFilteredProducts();
    }

    partial void OnSearchQueryChanged(string value)
    {
        UpdateFilteredProducts();
    }

    partial void OnSelectedCategoryChanged(string? value)
    {
        UpdateFilteredProducts();
    }

    partial void OnMinPriceChanged(string value)
    {
        UpdateFilteredProducts();
    }

    partial void OnMaxPriceChanged(string value)
    {
        UpdateFilteredProducts();
    }

    partial void OnSelectedSortOptionChanged(string? value)
    {
        UpdateFilteredProducts();
    }

    private void LoadMockData()
    {
        // Load Categories
        var categories = new List<string>
        {
            "Electronics",
            "Footwear",
            "Accessories",
            "Clothing",
            "Home & Garden"
        };
        
        foreach (var category in categories)
        {
            Categories.Add(category);
        }

        // Load Products
        _allProducts = new List<ProductViewModel>
        {
            new ProductViewModel
            {
                Id = 1,
                Sku = "ID: #065499",
                Name = "Black Urban Backpack",
                Category = "Accessories",
                Price = 101.00m,
                Stock = 240,
                Status = "Published",
                ImageUrl = "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=150&h=150&fit=crop"
            },
            new ProductViewModel
            {
                Id = 2,
                Sku = "ID: #065500",
                Name = "Nike Air Red",
                Category = "Footwear",
                Price = 144.50m,
                Stock = 12,
                Status = "Low Stock",
                ImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=150&h=150&fit=crop"
            },
            new ProductViewModel
            {
                Id = 3,
                Sku = "ID: #065501",
                Name = "Minimalist White Watch",
                Category = "Accessories",
                Price = 121.00m,
                Stock = 85,
                Status = "Published",
                ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=150&h=150&fit=crop"
            },
            new ProductViewModel
            {
                Id = 4,
                Sku = "ID: #065502",
                Name = "Sony ANC Headphones",
                Category = "Electronics",
                Price = 299.00m,
                Stock = 0,
                Status = "Out of Stock",
                ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=150&h=150&fit=crop"
            },
            new ProductViewModel
            {
                Id = 5,
                Sku = "ID: #065503",
                Name = "Gray Essential Hoodie",
                Category = "Clothing",
                Price = 45.00m,
                Stock = 320,
                Status = "Draft",
                ImageUrl = "https://images.unsplash.com/photo-1556821840-3a63f95609a7?w=150&h=150&fit=crop"
            },
            new ProductViewModel
            {
                Id = 6,
                Sku = "ID: #065504",
                Name = "MacBook Pro 16\"",
                Category = "Electronics",
                Price = 2499.00m,
                Stock = 45,
                Status = "Published",
                ImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=150&h=150&fit=crop"
            },
            new ProductViewModel
            {
                Id = 7,
                Sku = "ID: #065505",
                Name = "Leather Wallet",
                Category = "Accessories",
                Price = 89.99m,
                Stock = 156,
                Status = "Published",
                ImageUrl = "https://images.unsplash.com/photo-1627123424574-724758594e93?w=150&h=150&fit=crop"
            },
            new ProductViewModel
            {
                Id = 8,
                Sku = "ID: #065506",
                Name = "Running Shoes Pro",
                Category = "Footwear",
                Price = 129.00m,
                Stock = 8,
                Status = "Low Stock",
                ImageUrl = "https://images.unsplash.com/photo-1460353581641-37baddab0fa2?w=150&h=150&fit=crop"
            },
            new ProductViewModel
            {
                Id = 9,
                Sku = "ID: #065507",
                Name = "Wireless Mouse",
                Category = "Electronics",
                Price = 49.99m,
                Stock = 0,
                Status = "Out of Stock",
                ImageUrl = "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=150&h=150&fit=crop"
            },
            new ProductViewModel
            {
                Id = 10,
                Sku = "ID: #065508",
                Name = "Denim Jacket",
                Category = "Clothing",
                Price = 79.99m,
                Stock = 67,
                Status = "Published",
                ImageUrl = "https://images.unsplash.com/photo-1551028719-00167b16eac5?w=150&h=150&fit=crop"
            },
            new ProductViewModel
            {
                Id = 11,
                Sku = "ID: #065509",
                Name = "Sunglasses Premium",
                Category = "Accessories",
                Price = 159.00m,
                Stock = 95,
                Status = "Published",
                ImageUrl = "https://images.unsplash.com/photo-1572635196237-14b3f281503f?w=150&h=150&fit=crop"
            },
            new ProductViewModel
            {
                Id = 12,
                Sku = "ID: #065510",
                Name = "Smart Watch Series 8",
                Category = "Electronics",
                Price = 399.00m,
                Stock = 34,
                Status = "Published",
                ImageUrl = "https://images.unsplash.com/photo-1579586337278-3befd40fd17a?w=150&h=150&fit=crop"
            }
        };

        // Calculate stats
        TotalProducts = _allProducts.Count;
        TotalCategories = categories.Count;
        LowStockCount = _allProducts.Count(p => p.Status == "Low Stock");
        OutOfStockCount = _allProducts.Count(p => p.Status == "Out of Stock");
        TotalProductsCount = _allProducts.Count;
    }

    private void UpdateFilteredProducts()
    {
        var filtered = _allProducts.AsEnumerable();

        // Filter by search query
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            filtered = filtered.Where(p => 
                p.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                p.Sku.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
        }

        // Filter by category
        if (!string.IsNullOrWhiteSpace(SelectedCategory))
        {
            filtered = filtered.Where(p => p.Category == SelectedCategory);
        }

        // Filter by price range
        if (decimal.TryParse(MinPrice, out var minPrice))
        {
            filtered = filtered.Where(p => p.Price >= minPrice);
        }

        if (decimal.TryParse(MaxPrice, out var maxPrice))
        {
            filtered = filtered.Where(p => p.Price <= maxPrice);
        }

        // Sort
        filtered = SelectedSortOption switch
        {
            "Price: Low to High" => filtered.OrderBy(p => p.Price),
            "Price: High to Low" => filtered.OrderByDescending(p => p.Price),
            "Best Selling" => filtered.OrderByDescending(p => p.Stock),
            _ => filtered.OrderByDescending(p => p.Id) // Newest Added
        };

        _filteredList = filtered.ToList();
        TotalProductsCount = _filteredList.Count;
        
        // Reset to page 1 if current page exceeds total pages
        if (CurrentPage > TotalPages)
        {
            CurrentPage = 1;
        }
        
        ApplyPagination();
    }
    
    private void ApplyPagination()
    {
        // Apply pagination
        var pagedProducts = _filteredList
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        FilteredProducts.Clear();
        foreach (var product in pagedProducts)
        {
            FilteredProducts.Add(product);
        }

        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(PageStart));
        OnPropertyChanged(nameof(PageEnd));
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
        
        UpdatePageNumbers();
    }
    
    private void UpdatePageNumbers()
    {
        PageNumbers.Clear();
        
        int totalPages = TotalPages;
        int current = CurrentPage;
        
        // Always show first page
        PageNumbers.Add(new PageButtonModel { PageNumber = 1, IsCurrentPage = current == 1 });
        
        // Calculate range around current page
        int startPage = Math.Max(2, current - 1);
        int endPage = Math.Min(totalPages - 1, current + 1);
        
        // Add ellipsis if needed before range
        if (startPage > 2)
        {
            PageNumbers.Add(new PageButtonModel { IsEllipsis = true });
        }
        
        // Add pages around current
        for (int i = startPage; i <= endPage; i++)
        {
            PageNumbers.Add(new PageButtonModel { PageNumber = i, IsCurrentPage = current == i });
        }
        
        // Add ellipsis if needed after range
        if (endPage < totalPages - 1)
        {
            PageNumbers.Add(new PageButtonModel { IsEllipsis = true });
        }
        
        // Always show last page if more than 1 page
        if (totalPages > 1)
        {
            PageNumbers.Add(new PageButtonModel { PageNumber = totalPages, IsCurrentPage = current == totalPages });
        }
    }
    
    [RelayCommand]
    private void GoToPage(int pageNumber)
    {
        if (pageNumber >= 1 && pageNumber <= TotalPages && pageNumber != CurrentPage)
        {
            CurrentPage = pageNumber;
            ApplyPagination();
        }
    }

    [RelayCommand]
    private void ImportExcel()
    {
        // TODO: Implement Excel import
    }

    [RelayCommand]
    private async Task NewCategoryAsync()
    {
        var dialog = new Views.Products.AddCategoryDialog();
        dialog.XamlRoot = App.Current.MainWindow?.Content?.XamlRoot;

        var result = await dialog.ShowAsync();

        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary && dialog.ViewModel != null)
        {
            var vm = dialog.ViewModel;
            
            // Add new category to the list
            if (!Categories.Contains(vm.Name))
            {
                Categories.Add(vm.Name);
                TotalCategories = Categories.Count;
            }
        }
    }

    [RelayCommand]
    private async Task AddProductAsync()
    {
        var dialog = new Views.Products.AddProductDialog();
        dialog.XamlRoot = App.Current.MainWindow?.Content?.XamlRoot;
        
        var result = await dialog.ShowAsync();

        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary && dialog.ViewModel != null)
        {
            var vm = dialog.ViewModel;
            
            
            // Create new product from dialog data
            var newProduct = new ProductViewModel
            {
                Id = _allProducts.Count > 0 ? _allProducts.Max(p => p.Id) + 1 : 1,
                Sku = $"ID: #{vm.Sku}",
                Name = vm.Name,
                Category = vm.SelectedCategory ?? "Uncategorized",
                Price = (decimal)vm.SalePrice,
                Stock = (int)vm.Stock,
                Status = vm.SelectedStatus,
                ImageUrl = vm.ProductImages.FirstOrDefault()?.FilePath ?? "https://via.placeholder.com/150"
            };

            _allProducts.Insert(0, newProduct);
            
            // Update stats
            TotalProducts = _allProducts.Count;
            LowStockCount = _allProducts.Count(p => p.Status == "Low Stock");
            OutOfStockCount = _allProducts.Count(p => p.Status == "Out of Stock");
            
            UpdateFilteredProducts();
        }
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CanGoPrevious)
        {
            CurrentPage--;
            ApplyPagination();
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CanGoNext)
        {
            CurrentPage++;
            ApplyPagination();
        }
    }
}

public partial class ProductViewModel : ObservableObject
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
    private decimal _price;

    [ObservableProperty]
    private int _stock;

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private string _imageUrl = "https://via.placeholder.com/150";

    [ObservableProperty]
    private bool _isSelected;

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
/// Model for pagination button - either a page number or ellipsis
/// </summary>
public partial class PageButtonModel : ObservableObject
{
    [ObservableProperty]
    private int _pageNumber;
    
    [ObservableProperty]
    private bool _isCurrentPage;
    
    [ObservableProperty]
    private bool _isEllipsis;
    
    public string DisplayText => IsEllipsis ? "..." : PageNumber.ToString();
    
    public string ButtonBackground => IsCurrentPage ? "#7C5CFC" : "#F9FAFB";
    
    public string ButtonForeground => IsCurrentPage ? "#FFFFFF" : "#6B7280";
}


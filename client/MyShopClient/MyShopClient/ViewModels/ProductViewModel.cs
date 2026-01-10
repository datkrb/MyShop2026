using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.Models;
using MyShopClient.Services.Api;
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
    private readonly ProductApiService _productApiService;

    [ObservableProperty]
    private ObservableCollection<ApiProduct> _products = new();

    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();

    [ObservableProperty]
    private Category? _selectedCategory;

    // Filter Trigger
    async partial void OnSelectedCategoryChanged(Category? value)
    {
        _currentPage = 1;
        await LoadProducts();
    }

    [ObservableProperty]
    private string _searchKeyword = string.Empty;

    // Sort Configuration
    public List<string> SortOptions { get; } = new List<string>
    {
        "Mới nhất",
        "Giá: Thấp -> Cao",
        "Giá: Cao -> Thấp",
        "Tên: A -> Z"
    };

    [ObservableProperty]
    private string _selectedSortOption = "Mới nhất";

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

    public ProductViewModel(ProductApiService productApiService)
    {
        _productApiService = productApiService;
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
        
        var list = new List<Category> { new Category { Id = -1, Name = "Tất cả" } };
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
            "Mới nhất" => "id,desc",
            "Giá: Thấp -> Cao" => "salePrice,asc",
            "Giá: Cao -> Thấp" => "salePrice,desc",
            "Tên: A -> Z" => "name,asc",
            _ => "id,desc"
        };
    }

    [RelayCommand]
    public async Task LoadProducts()
    {
        IsLoading = true;
        try 
        {
            int? catId = (SelectedCategory == null || SelectedCategory.Id == -1) ? null : SelectedCategory.Id;
            string sort = GetBackendSort(SelectedSortOption);

            var result = await _productApiService.GetProductsAsync(CurrentPage, PageSize, catId, SearchKeyword, sort);
            
            if (result != null)
            {
                Products = new ObservableCollection<ApiProduct>(result.Data);
                TotalItems = result.Total;
                TotalPages = result.TotalPages;
                CurrentPage = result.Page;

                UpdatePagingState();
            }
        }
        catch (Exception ex)
        {
            // Handle error (show dialog/toast)
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

    [RelayCommand]
    public async Task Search()
    {
        _currentPage = 1;
        await LoadProducts();
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
            else 
            {
                 // Show error
            }
        }
    }

    [RelayCommand]
    public void ViewDetail(ApiProduct product)
    {
        // Navigate or Show Dialog. 
        // User asked for "View List -> View Detail -> Delete/Edit" flow.
        // I will implement navigation to a Detail Page.
        // Assuming ShellPage frame navigation.
        
        // Passing ID as parameter
        App.Current.ContentFrame?.Navigate(typeof(Views.Products.ProductDetailPage), product.Id);
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.Models;
using MyShopClient.Services.Api;
using MyShopClient.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;

namespace MyShopClient.ViewModels;

public partial class ProductSelectionViewModel : ViewModelBase
{
    private readonly ProductApiService _productApiService;
    private System.Threading.CancellationTokenSource? _searchDebounceToken;

    [ObservableProperty]
    private string _searchKeyword = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ApiProduct> _products = new();

    [ObservableProperty]
    private ApiProduct? _selectedProduct;

    [ObservableProperty]
    private bool _isLoading;

    // Pagination
    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private int _totalPages = 0;

    [ObservableProperty]
    private int _totalItems = 0;

    // [ObservableProperty]
    // private ObservableCollection<PageInfo> _pages = new();

    // PageNumbers for PaginationControl compatibility
    public ObservableCollection<PageButtonModel> PageNumbers { get; } = new();

    [ObservableProperty]
    private bool _canGoNext;

    [ObservableProperty]
    private bool _canGoPrev;

    public ProductSelectionViewModel(ProductApiService productApiService)
    {
        _productApiService = productApiService;
    }

    [RelayCommand]
    public async Task Search()
    {
        CurrentPage = 1;
        await LoadProducts();
    }

    partial void OnSearchKeywordChanged(string value)
    {
        _searchDebounceToken?.Cancel();
        _searchDebounceToken = new System.Threading.CancellationTokenSource();
        var token = _searchDebounceToken.Token;

        _ = DebounceSearchAsync(token);
    }

    private async Task DebounceSearchAsync(System.Threading.CancellationToken token)
    {
        try
        {
            await Task.Delay(500, token);
            if (!token.IsCancellationRequested)
            {
                CurrentPage = 1;
                await LoadProducts();
            }
        }
        catch (TaskCanceledException) { }
    }

    public async Task LoadProducts()
    {
        IsLoading = true;
        try
        {
            var result = await _productApiService.GetProductsAsync(CurrentPage, PageSize, null, SearchKeyword);
            
            if (result != null)
            {
                Products = new ObservableCollection<ApiProduct>(result.Data);
                TotalItems = result.Total;
                TotalPages = result.TotalPages;
                CurrentPage = result.Page;

                UpdatePagingState();
                GeneratePagination();
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
        PageNumbers.Clear();

        if (TotalPages <= 1) return;

        PageNumbers.Add(new PageButtonModel { PageNumber = 1, IsCurrentPage = CurrentPage == 1 });

        int startPage = Math.Max(2, CurrentPage - 1);
        int endPage = Math.Min(TotalPages - 1, CurrentPage + 1);

        if (startPage > 2)
        {
            PageNumbers.Add(new PageButtonModel { IsEllipsis = true });
        }

        for (int i = startPage; i <= endPage; i++)
        {
            PageNumbers.Add(new PageButtonModel { PageNumber = i, IsCurrentPage = CurrentPage == i });
        }

        if (endPage < TotalPages - 1)
        {
            PageNumbers.Add(new PageButtonModel { IsEllipsis = true });
        }

        if (TotalPages > 1)
        {
            PageNumbers.Add(new PageButtonModel { PageNumber = TotalPages, IsCurrentPage = CurrentPage == TotalPages });
        }
        
        // Update unused Pages collection just in case, or we can remove it if not used in XAML (it wasn't used in the XAML snippet I saw)
        // referencing clean up 
    }

    private async Task GoToPage(int page)
    {
        if (page == CurrentPage) return;
        CurrentPage = page;
        await LoadProducts();
    }

    public async Task GoToPageAsync(int page)
    {
        await GoToPage(page);
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
}

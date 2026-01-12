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

    [ObservableProperty]
    private ObservableCollection<PageInfo> _pages = new();

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
        var list = new ObservableCollection<PageInfo>();
        if (TotalPages <= 5)
        {
            for (int i = 1; i <= TotalPages; i++)
                list.Add(CreatePageInfo(i));
        }
        else
        {
            if (CurrentPage <= 2)
            {
                list.Add(CreatePageInfo(1));
                list.Add(CreatePageInfo(2));
                list.Add(new PageInfo { Text = "...", IsEnabled = false });
                list.Add(CreatePageInfo(TotalPages));
            }
            else if (CurrentPage >= TotalPages - 1)
            {
                list.Add(CreatePageInfo(1));
                list.Add(new PageInfo { Text = "...", IsEnabled = false });
                list.Add(CreatePageInfo(TotalPages - 1));
                list.Add(CreatePageInfo(TotalPages));
            }
            else
            {
                list.Add(CreatePageInfo(1));
                list.Add(new PageInfo { Text = "...", IsEnabled = false });
                list.Add(CreatePageInfo(CurrentPage));
                list.Add(new PageInfo { Text = "...", IsEnabled = false });
                list.Add(CreatePageInfo(TotalPages));
            }
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

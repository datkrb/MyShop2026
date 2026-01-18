using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.Models;
using MyShopClient.Services.Api;
using MyShopClient.Services.Config;
using MyShopClient.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShopClient.ViewModels;

public partial class CustomersViewModel : ViewModelBase
{
    private readonly CustomerApiService _customerApiService;

    // Loading state
    [ObservableProperty]
    private bool _isLoading;

    // Search
    [ObservableProperty]
    private string _searchQuery = string.Empty;

    // Stats
    [ObservableProperty]
    private int _totalCustomers;

    [ObservableProperty]
    private int _newThisMonth;

    [ObservableProperty]
    private int _newThisWeek;

    // Selection mode (for dialog)
    [ObservableProperty]
    private bool _isSelectionMode;

    [ObservableProperty]
    private CustomerViewModel? _selectedCustomer;

    // Advanced Search Filters
    [ObservableProperty]
    private bool? _hasOrders = null;

    [ObservableProperty]
    private DateTimeOffset? _createdFrom;

    [ObservableProperty]
    private DateTimeOffset? _createdTo;

    [ObservableProperty]
    private string _selectedSort = "name,asc";

    public List<string> SortOptions { get; } = new List<string>
    {
        "name,asc",
        "name,desc",
        "createdAt,desc",
        "createdAt,asc"
    };

    public List<string> HasOrdersOptions { get; } = new List<string>
    {
        "All",
        "With Orders",
        "Without Orders"
    };

    // Pagination
    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private int _totalPages = 1;

    public int PageStart => TotalCustomers > 0 ? (CurrentPage - 1) * PageSize + 1 : 0;
    public int PageEnd => Math.Min(CurrentPage * PageSize, TotalCustomers);
    public bool CanGoPrevious => CurrentPage > 1;
    public bool CanGoNext => CurrentPage < TotalPages;

    public ObservableCollection<CustomerViewModel> FilteredCustomers { get; } = new();
    public ObservableCollection<PageButtonModel> PageNumbers { get; } = new();

    // Debounce timer for search
    private System.Threading.CancellationTokenSource? _searchDebounceToken;
    private readonly AppSettingsService _appSettingsService;

    public CustomersViewModel(CustomerApiService customerApiService, AppSettingsService appSettingsService)
    {
        _customerApiService = customerApiService ?? throw new ArgumentNullException(nameof(customerApiService));
        _appSettingsService = appSettingsService;
        
        // Load PageSize from settings
        _pageSize = _appSettingsService.GetPageSize();
    }

    public async Task LoadCustomersAsync()
    {
        IsLoading = true;

        try
        {
            // Convert HasOrders to bool
            bool? hasOrdersValue = null;
            // Using HasOrders directly if it's set

            var result = await _customerApiService.GetCustomersAsync(
                page: CurrentPage,
                size: PageSize,
                keyword: string.IsNullOrWhiteSpace(SearchQuery) ? null : SearchQuery,
                hasOrders: HasOrders,
                createdFrom: CreatedFrom?.ToString("yyyy-MM-dd"),
                createdTo: CreatedTo?.ToString("yyyy-MM-dd"),
                sort: SelectedSort
            );

            if (result != null)
            {
                TotalCustomers = result.Total;
                TotalPages = result.TotalPages > 0 ? result.TotalPages : 1;

                FilteredCustomers.Clear();
                foreach (var customer in result.Data)
                {
                    FilteredCustomers.Add(new CustomerViewModel
                    {
                        Id = customer.Id,
                        Name = customer.Name,
                        Email = customer.Email,
                        Phone = customer.Phone,
                        Address = customer.Address,
                        CreatedAt = customer.CreatedAt
                    });
                }

                // Load statistics from API
                await LoadStatsAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading customers: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            UpdatePaginationProperties();
            UpdatePageNumbers();
        }
    }

    private async Task LoadStatsAsync()
    {
        try
        {
            var stats = await _customerApiService.GetStatsAsync();
            if (stats != null)
            {
                TotalCustomers = stats.Total;
                NewThisMonth = stats.NewThisMonth;
                NewThisWeek = stats.NewThisWeek;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading customer stats: {ex.Message}");
        }
    }

    partial void OnSearchQueryChanged(string value)
    {
        // Debounce search to avoid too many API calls
        _searchDebounceToken?.Cancel();
        _searchDebounceToken = new System.Threading.CancellationTokenSource();

        var token = _searchDebounceToken.Token;

        _ = DebounceSearchAsync(token);
    }

    private async Task DebounceSearchAsync(System.Threading.CancellationToken token)
    {
        try
        {
            await Task.Delay(400, token);
            
            if (!token.IsCancellationRequested)
            {
                CurrentPage = 1;
                await LoadCustomersAsync();
            }
        }
        catch (TaskCanceledException)
        {
            // Ignored - expected when user types again before delay completes
        }
    }

    private void UpdatePaginationProperties()
    {
        OnPropertyChanged(nameof(PageStart));
        OnPropertyChanged(nameof(PageEnd));
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
    }

    private void UpdatePageNumbers()
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
    }

    [RelayCommand]
    public async Task GoToPageAsync(int pageNumber)
    {
        if (pageNumber >= 1 && pageNumber <= TotalPages && pageNumber != CurrentPage)
        {
            CurrentPage = pageNumber;
            await LoadCustomersAsync();
        }
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CanGoPrevious)
        {
            CurrentPage--;
            await LoadCustomersAsync();
        }
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CanGoNext)
        {
            CurrentPage++;
            await LoadCustomersAsync();
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadCustomersAsync();
    }

    [RelayCommand]
    private async Task DeleteCustomerAsync(int customerId)
    {
        try
        {
            var success = await _customerApiService.DeleteCustomerAsync(customerId);
            if (success)
            {
                await LoadCustomersAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting customer: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchQuery = string.Empty;
        HasOrders = null;
        CreatedFrom = null;
        CreatedTo = null;
        SelectedSort = "name,asc";
        CurrentPage = 1;
        _ = LoadCustomersAsync();
    }
}

/// <summary>
/// Customer ViewModel for display in list
/// </summary>
public partial class CustomerViewModel : ObservableObject
{
    public int Id { get; set; }
    
    [ObservableProperty]
    private string _name = string.Empty;
    
    [ObservableProperty]
    private string? _email;
    
    [ObservableProperty]
    private string? _phone;
    
    [ObservableProperty]
    private string? _address;
    
    [ObservableProperty]
    private DateTime _createdAt;

    public string AvatarUrl => $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(Name)}&background=7C5CFC&color=fff";
    public string FormattedCreatedDate => CreatedAt.ToString("MMM dd, yyyy");
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.Models;
using MyShopClient.Services.Api;
using MyShopClient.Services.Config;
using MyShopClient.Services.Navigation;
using MyShopClient.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShopClient.ViewModels;

public partial class OrdersViewModel : ViewModelBase
{
    private readonly OrderApiService _orderApiService;
    private readonly INavigationService _navigationService;

    // Loading state
    [ObservableProperty]
    private bool _isLoading;

    // Search and Filters
    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? _fromDate = null;

    [ObservableProperty]
    private DateTimeOffset? _toDate = null;

    [ObservableProperty]
    private string _selectedOrderStatus = "All";

    // Stats
    [ObservableProperty]
    private int _draftCount;

    [ObservableProperty]
    private int _pendingCount;

    [ObservableProperty]
    private int _paidCount;

    [ObservableProperty]
    private int _cancelledCount;

    [ObservableProperty]
    private int _totalOrdersCount;

    // Pagination
    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private int _totalPages = 1;

    public int PageStart => TotalOrdersCount > 0 ? (CurrentPage - 1) * PageSize + 1 : 0;
    public int PageEnd => Math.Min(CurrentPage * PageSize, TotalOrdersCount);
    public bool CanGoPrevious => CurrentPage > 1;
    public bool CanGoNext => CurrentPage < TotalPages;

    public ObservableCollection<string> OrderStatuses { get; } = new()
    {
        "All", "DRAFT", "PENDING", "PAID", "CANCELLED"
    };

    public ObservableCollection<OrderViewModel> FilteredOrders { get; } = new();
    public ObservableCollection<PageButtonModel> PageNumbers { get; } = new();

    // Debounce
    private System.Threading.CancellationTokenSource? _searchDebounceToken;
    private readonly AppSettingsService _appSettingsService;

    public OrdersViewModel(OrderApiService orderApiService, AppSettingsService appSettingsService, INavigationService navigationService)
    {
        _orderApiService = orderApiService ?? throw new ArgumentNullException(nameof(orderApiService));
        _appSettingsService = appSettingsService;
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        
        // Load PageSize from settings
        _pageSize = _appSettingsService.GetPageSize();
    }

    public async Task LoadOrdersAsync()
    {
        IsLoading = true;

        try
        {
            var result = await _orderApiService.GetOrdersAsync(
                page: CurrentPage,
                size: PageSize,
                status: SelectedOrderStatus == "All" ? null : SelectedOrderStatus,
                fromDate: FromDate?.DateTime,
                toDate: ToDate?.DateTime
            );

            if (result != null)
            {
                TotalOrdersCount = result.Total;
                TotalPages = result.TotalPages > 0 ? result.TotalPages : 1;

                FilteredOrders.Clear();
                foreach (var order in result.Data)
                {
                    FilteredOrders.Add(new OrderViewModel
                    {
                        Id = order.Id,
                        OrderId = $"#{order.Id:D4}",
                        CustomerName = order.Customer?.Name ?? "Walk-in Customer",
                        CustomerEmail = order.Customer?.Email ?? "",
                        CustomerAvatar = order.Customer?.AvatarUrl ?? $"https://ui-avatars.com/api/?name=Guest&background=7C5CFC&color=fff",
                        OrderDate = order.CreatedTime,
                        Amount = order.FinalPrice,
                        OrderStatus = order.Status
                    });
                }

                // Update stats (simplified - would need separate API for accurate counts)
                DraftCount = FilteredOrders.Count(o => o.OrderStatus == "DRAFT");
                PendingCount = FilteredOrders.Count(o => o.OrderStatus == "PENDING");
                PaidCount = FilteredOrders.Count(o => o.OrderStatus == "PAID");
                CancelledCount = FilteredOrders.Count(o => o.OrderStatus == "CANCELLED");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading orders: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            UpdatePaginationProperties();
            UpdatePageNumbers();
        }
    }

    partial void OnSearchQueryChanged(string value)
    {
        // Debounce search
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
                await LoadOrdersAsync();
            }
        }
        catch (TaskCanceledException) { }
    }

    partial void OnFromDateChanged(DateTimeOffset? value)
    {
        CurrentPage = 1;
        _ = LoadOrdersAsync();
    }

    partial void OnToDateChanged(DateTimeOffset? value)
    {
        CurrentPage = 1;
        _ = LoadOrdersAsync();
    }

    partial void OnSelectedOrderStatusChanged(string value)
    {
        CurrentPage = 1;
        _ = LoadOrdersAsync();
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
            await LoadOrdersAsync();
        }
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CanGoPrevious)
        {
            CurrentPage--;
            await LoadOrdersAsync();
        }
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CanGoNext)
        {
            CurrentPage++;
            await LoadOrdersAsync();
        }
    }

    [RelayCommand]
    private void CreateOrder()
    {
        _navigationService.Navigate(typeof(Views.Orders.OrderDetailView), "new");
    }

    [RelayCommand]
    private void EditOrder(OrderViewModel order)
    {
        if (order != null)
        {
            _navigationService.Navigate(typeof(Views.Orders.OrderDetailView), order.Id);
        }
    }

    [RelayCommand]
    public async Task DeleteOrderAsync(OrderViewModel order)
    {
        if (order != null)
        {
            try
            {
                var success = await _orderApiService.DeleteOrderAsync(order.Id);
                if (success)
                {
                    await LoadOrdersAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting order: {ex.Message}");
            }
        }
    }

    [RelayCommand]
    private void ViewOrder(OrderViewModel order)
    {
        if (order != null)
        {
            _navigationService.Navigate(typeof(Views.Orders.OrderDetailView), order.Id);
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadOrdersAsync();
    }
}

/// <summary>
/// ViewModel for order table row
/// </summary>
public partial class OrderViewModel : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _orderId = string.Empty;

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private string _customerEmail = string.Empty;

    [ObservableProperty]
    private string _customerAvatar = string.Empty;

    [ObservableProperty]
    private DateTime _orderDate;

    [ObservableProperty]
    private decimal _amount;

    [ObservableProperty]
    private string _orderStatus = string.Empty;

    [ObservableProperty]
    private bool _isSelected;

    // Computed properties for status styling
    public string OrderStatusBackground => OrderStatus switch
    {
        "DRAFT" => "#F3F4F6",
        "PENDING" => "#FEF3C7",
        "PAID" => "#DCFCE7",
        "CANCELLED" => "#FEE2E2",
        _ => "#F3F4F6"
    };

    public string OrderStatusForeground => OrderStatus switch
    {
        "DRAFT" => "#4B5563",
        "PENDING" => "#CA8A04",
        "PAID" => "#15803D",
        "CANCELLED" => "#B91C1C",
        _ => "#4B5563"
    };

    public string FormattedDate => OrderDate.ToString("dd/MM/yyyy");
    public string FormattedAmount => $"{Amount:N0}đ";
    
    public string DisplayStatus => OrderStatus switch
    {
        "DRAFT" => "Draft",
        "PENDING" => "Pending",
        "PAID" => "Paid",
        "CANCELLED" => "Cancelled",
        _ => OrderStatus
    };
}

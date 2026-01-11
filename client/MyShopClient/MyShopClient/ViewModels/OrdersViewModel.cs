using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.Models;
using MyShopClient.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MyShopClient.ViewModels;

public partial class OrdersViewModel : ViewModelBase
{
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
    private int _newOrdersCount;

    [ObservableProperty]
    private int _pendingPaymentCount;

    [ObservableProperty]
    private int _deliveredCount;

    [ObservableProperty]
    private decimal _totalRevenue;

    [ObservableProperty]
    private int _totalOrdersCount;

    // Pagination
    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    public int TotalPages => TotalOrdersCount > 0 ? (int)Math.Ceiling((double)TotalOrdersCount / PageSize) : 1;
    public int PageStart => TotalOrdersCount > 0 ? (CurrentPage - 1) * PageSize + 1 : 0;
    public int PageEnd => Math.Min(CurrentPage * PageSize, TotalOrdersCount);
    public bool CanGoPrevious => CurrentPage > 1;
    public bool CanGoNext => CurrentPage < TotalPages;

    public ObservableCollection<string> OrderStatuses { get; } = new()
    {
        "All", "New", "Paid", "Canceled"
    };

    public ObservableCollection<OrderViewModel> Orders { get; } = new();
    public ObservableCollection<OrderViewModel> FilteredOrders { get; } = new();
    public ObservableCollection<PageButtonModel> PageNumbers { get; } = new();

    private List<OrderViewModel> _allOrders = new();

    public OrdersViewModel()
    {
        LoadMockData();
        UpdateFilteredOrders();
    }

    private void LoadMockData()
    {
        _allOrders = new List<OrderViewModel>
        {
            new() { Id = 1, OrderId = "#ORD-2023-001", CustomerName = "Sarah Smith", CustomerEmail = "sarah@example.com", CustomerAvatar = "https://ui-avatars.com/api/?name=Sarah+Smith&background=7C5CFC&color=fff", OrderDate = DateTime.Now.AddDays(-1), Amount = 120.50m, OrderStatus = "New" },
            new() { Id = 2, OrderId = "#ORD-2023-002", CustomerName = "Michael Brown", CustomerEmail = "michael.b@tech.com", CustomerAvatar = "https://ui-avatars.com/api/?name=Michael+Brown&background=10B981&color=fff", OrderDate = DateTime.Now.AddDays(-2), Amount = 850.00m, OrderStatus = "Paid" },
            new() { Id = 3, OrderId = "#ORD-2023-003", CustomerName = "Emily Davis", CustomerEmail = "emily.design@studio.io", CustomerAvatar = "https://ui-avatars.com/api/?name=Emily+Davis&background=EF4444&color=fff", OrderDate = DateTime.Now.AddDays(-3), Amount = 45.00m, OrderStatus = "Canceled" },
            new() { Id = 4, OrderId = "#ORD-2023-004", CustomerName = "James Wilson", CustomerEmail = "jwilson@corp.net", CustomerAvatar = "https://ui-avatars.com/api/?name=James+Wilson&background=F59E0B&color=fff", OrderDate = DateTime.Now.AddDays(-4), Amount = 2300.00m, OrderStatus = "Paid" },
            new() { Id = 5, OrderId = "#ORD-2023-005", CustomerName = "Anna Johnson", CustomerEmail = "anna.j@mail.com", CustomerAvatar = "https://ui-avatars.com/api/?name=Anna+Johnson&background=7C5CFC&color=fff", OrderDate = DateTime.Now.AddDays(-5), Amount = 175.25m, OrderStatus = "New" },
            new() { Id = 6, OrderId = "#ORD-2023-006", CustomerName = "Robert Lee", CustomerEmail = "robert.lee@business.com", CustomerAvatar = "https://ui-avatars.com/api/?name=Robert+Lee&background=10B981&color=fff", OrderDate = DateTime.Now.AddDays(-5), Amount = 550.00m, OrderStatus = "Paid" },
            new() { Id = 7, OrderId = "#ORD-2023-007", CustomerName = "Lisa Chen", CustomerEmail = "lisa.chen@shop.com", CustomerAvatar = "https://ui-avatars.com/api/?name=Lisa+Chen&background=EF4444&color=fff", OrderDate = DateTime.Now.AddDays(-6), Amount = 89.99m, OrderStatus = "New" },
            new() { Id = 8, OrderId = "#ORD-2023-008", CustomerName = "David Kim", CustomerEmail = "david.k@email.org", CustomerAvatar = "https://ui-avatars.com/api/?name=David+Kim&background=F59E0B&color=fff", OrderDate = DateTime.Now.AddDays(-7), Amount = 420.00m, OrderStatus = "Canceled" },
            // Additional mock data for pagination
            new() { Id = 9, OrderId = "#ORD-2023-009", CustomerName = "Jennifer White", CustomerEmail = "j.white@company.com", CustomerAvatar = "https://ui-avatars.com/api/?name=Jennifer+White&background=7C5CFC&color=fff", OrderDate = DateTime.Now.AddDays(-8), Amount = 299.99m, OrderStatus = "Paid" },
            new() { Id = 10, OrderId = "#ORD-2023-010", CustomerName = "Thomas Garcia", CustomerEmail = "t.garcia@mail.com", CustomerAvatar = "https://ui-avatars.com/api/?name=Thomas+Garcia&background=10B981&color=fff", OrderDate = DateTime.Now.AddDays(-9), Amount = 1250.00m, OrderStatus = "New" },
            new() { Id = 11, OrderId = "#ORD-2023-011", CustomerName = "Patricia Martinez", CustomerEmail = "p.martinez@work.com", CustomerAvatar = "https://ui-avatars.com/api/?name=Patricia+Martinez&background=EF4444&color=fff", OrderDate = DateTime.Now.AddDays(-10), Amount = 78.50m, OrderStatus = "Paid" },
            new() { Id = 12, OrderId = "#ORD-2023-012", CustomerName = "Christopher Lee", CustomerEmail = "c.lee@tech.io", CustomerAvatar = "https://ui-avatars.com/api/?name=Christopher+Lee&background=F59E0B&color=fff", OrderDate = DateTime.Now.AddDays(-11), Amount = 650.00m, OrderStatus = "New" },
            new() { Id = 13, OrderId = "#ORD-2023-013", CustomerName = "Amanda Taylor", CustomerEmail = "a.taylor@shop.com", CustomerAvatar = "https://ui-avatars.com/api/?name=Amanda+Taylor&background=7C5CFC&color=fff", OrderDate = DateTime.Now.AddDays(-12), Amount = 189.00m, OrderStatus = "Paid" },
            new() { Id = 14, OrderId = "#ORD-2023-014", CustomerName = "Daniel Anderson", CustomerEmail = "d.anderson@corp.net", CustomerAvatar = "https://ui-avatars.com/api/?name=Daniel+Anderson&background=10B981&color=fff", OrderDate = DateTime.Now.AddDays(-13), Amount = 3200.00m, OrderStatus = "Canceled" },
            new() { Id = 15, OrderId = "#ORD-2023-015", CustomerName = "Michelle Thomas", CustomerEmail = "m.thomas@email.org", CustomerAvatar = "https://ui-avatars.com/api/?name=Michelle+Thomas&background=EF4444&color=fff", OrderDate = DateTime.Now.AddDays(-14), Amount = 445.00m, OrderStatus = "New" },
            new() { Id = 16, OrderId = "#ORD-2023-016", CustomerName = "Kevin Jackson", CustomerEmail = "k.jackson@business.com", CustomerAvatar = "https://ui-avatars.com/api/?name=Kevin+Jackson&background=F59E0B&color=fff", OrderDate = DateTime.Now.AddDays(-15), Amount = 99.99m, OrderStatus = "Paid" },
            new() { Id = 17, OrderId = "#ORD-2023-017", CustomerName = "Nancy Moore", CustomerEmail = "n.moore@mail.com", CustomerAvatar = "https://ui-avatars.com/api/?name=Nancy+Moore&background=7C5CFC&color=fff", OrderDate = DateTime.Now.AddDays(-16), Amount = 780.00m, OrderStatus = "New" },
            new() { Id = 18, OrderId = "#ORD-2023-018", CustomerName = "Brian Harris", CustomerEmail = "b.harris@work.io", CustomerAvatar = "https://ui-avatars.com/api/?name=Brian+Harris&background=10B981&color=fff", OrderDate = DateTime.Now.AddDays(-17), Amount = 125.50m, OrderStatus = "Paid" },
            new() { Id = 19, OrderId = "#ORD-2023-019", CustomerName = "Elizabeth Clark", CustomerEmail = "e.clark@shop.com", CustomerAvatar = "https://ui-avatars.com/api/?name=Elizabeth+Clark&background=EF4444&color=fff", OrderDate = DateTime.Now.AddDays(-18), Amount = 2100.00m, OrderStatus = "Canceled" },
            new() { Id = 20, OrderId = "#ORD-2023-020", CustomerName = "George Lewis", CustomerEmail = "g.lewis@company.net", CustomerAvatar = "https://ui-avatars.com/api/?name=George+Lewis&background=F59E0B&color=fff", OrderDate = DateTime.Now.AddDays(-19), Amount = 350.00m, OrderStatus = "Paid" },
            new() { Id = 21, OrderId = "#ORD-2023-021", CustomerName = "Sandra Robinson", CustomerEmail = "s.robinson@tech.com", CustomerAvatar = "https://ui-avatars.com/api/?name=Sandra+Robinson&background=7C5CFC&color=fff", OrderDate = DateTime.Now.AddDays(-20), Amount = 890.00m, OrderStatus = "New" },
            new() { Id = 22, OrderId = "#ORD-2023-022", CustomerName = "Mark Walker", CustomerEmail = "m.walker@mail.org", CustomerAvatar = "https://ui-avatars.com/api/?name=Mark+Walker&background=10B981&color=fff", OrderDate = DateTime.Now.AddDays(-21), Amount = 67.99m, OrderStatus = "Paid" },
            new() { Id = 23, OrderId = "#ORD-2023-023", CustomerName = "Donna Hall", CustomerEmail = "d.hall@business.io", CustomerAvatar = "https://ui-avatars.com/api/?name=Donna+Hall&background=EF4444&color=fff", OrderDate = DateTime.Now.AddDays(-22), Amount = 1450.00m, OrderStatus = "New" },
            new() { Id = 24, OrderId = "#ORD-2023-024", CustomerName = "Steven Young", CustomerEmail = "s.young@shop.net", CustomerAvatar = "https://ui-avatars.com/api/?name=Steven+Young&background=F59E0B&color=fff", OrderDate = DateTime.Now.AddDays(-23), Amount = 520.00m, OrderStatus = "Canceled" },
            new() { Id = 25, OrderId = "#ORD-2023-025", CustomerName = "Carol King", CustomerEmail = "c.king@work.com", CustomerAvatar = "https://ui-avatars.com/api/?name=Carol+King&background=7C5CFC&color=fff", OrderDate = DateTime.Now.AddDays(-24), Amount = 275.00m, OrderStatus = "Paid" },
        };

        // Calculate stats
        NewOrdersCount = _allOrders.Count(o => o.OrderStatus == "New");
        PendingPaymentCount = _allOrders.Count(o => o.OrderStatus == "Paid");
        DeliveredCount = _allOrders.Count(o => o.OrderStatus == "Canceled");
        TotalRevenue = _allOrders.Sum(o => o.Amount);
        TotalOrdersCount = _allOrders.Count();
    }

    partial void OnSearchQueryChanged(string value) => UpdateFilteredOrders();
    partial void OnFromDateChanged(DateTimeOffset? value) => UpdateFilteredOrders();
    partial void OnToDateChanged(DateTimeOffset? value) => UpdateFilteredOrders();
    partial void OnSelectedOrderStatusChanged(string value) => UpdateFilteredOrders();

    private void UpdateFilteredOrders()
    {
        var filtered = _allOrders.AsEnumerable();

        // Filter by search query
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            filtered = filtered.Where(o =>
                o.OrderId.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                o.CustomerName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                o.CustomerEmail.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
        }

        // Filter by date range
        if (FromDate.HasValue)
        {
            var fromDateTime = FromDate.Value.Date;
            filtered = filtered.Where(o => o.OrderDate.Date >= fromDateTime);
        }

        if (ToDate.HasValue)
        {
            var toDateTime = ToDate.Value.Date;
            filtered = filtered.Where(o => o.OrderDate.Date <= toDateTime);
        }

        // Filter by order status
        if (SelectedOrderStatus != "All")
        {
            filtered = filtered.Where(o => o.OrderStatus == SelectedOrderStatus);
        }

        var filteredList = filtered.OrderByDescending(o => o.OrderDate).ToList();
        TotalOrdersCount = filteredList.Count();

        // Reset to page 1 if needed
        if (CurrentPage > TotalPages)
        {
            CurrentPage = 1;
        }

        ApplyPagination(filteredList);
    }

    private void ApplyPagination(List<OrderViewModel> filteredList)
    {
        var pagedOrders = filteredList
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        FilteredOrders.Clear();
        foreach (var order in pagedOrders)
        {
            FilteredOrders.Add(order);
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

        if (totalPages <= 1) return;

        PageNumbers.Add(new PageButtonModel { PageNumber = 1, IsCurrentPage = current == 1 });

        int startPage = Math.Max(2, current - 1);
        int endPage = Math.Min(totalPages - 1, current + 1);

        if (startPage > 2)
        {
            PageNumbers.Add(new PageButtonModel { IsEllipsis = true });
        }

        for (int i = startPage; i <= endPage; i++)
        {
            PageNumbers.Add(new PageButtonModel { PageNumber = i, IsCurrentPage = current == i });
        }

        if (endPage < totalPages - 1)
        {
            PageNumbers.Add(new PageButtonModel { IsEllipsis = true });
        }

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
            UpdateFilteredOrders();
        }
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CanGoPrevious)
        {
            CurrentPage--;
            UpdateFilteredOrders();
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CanGoNext)
        {
            CurrentPage++;
            UpdateFilteredOrders();
        }
    }

    [RelayCommand]
    private void CreateOrder()
    {
        // TODO: Open create order dialog
    }

    [RelayCommand]
    private void EditOrder(OrderViewModel order)
    {
        // TODO: Open edit order dialog
    }

    [RelayCommand]
    private void DeleteOrder(OrderViewModel order)
    {
        // TODO: Confirm and delete order
    }

    [RelayCommand]
    private void ViewOrder(OrderViewModel order)
    {
        // TODO: Navigate to order detail
    }

    [RelayCommand]
    private void Refresh()
    {
        LoadMockData();
        UpdateFilteredOrders();
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
        "New" => "#DBEAFE",
        "Paid" => "#DCFCE7",
        "Canceled" => "#FEE2E2",
        _ => "#F3F4F6"
    };

    public string OrderStatusForeground => OrderStatus switch
    {
        "New" => "#1D4ED8",
        "Paid" => "#15803D",
        "Canceled" => "#B91C1C",
        _ => "#4B5563"
    };

    public string FormattedDate => OrderDate.ToString("MMM dd, yyyy");
    public string FormattedAmount => $"${Amount:N2}";
}

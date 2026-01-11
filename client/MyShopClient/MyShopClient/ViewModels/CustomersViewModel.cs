using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using MyShopClient.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI;

namespace MyShopClient.ViewModels;

public partial class CustomersViewModel : ViewModelBase
{
    // Search
    [ObservableProperty]
    private string _searchQuery = string.Empty;

    // Stats
    [ObservableProperty]
    private int _totalCustomers;

    [ObservableProperty]
    private int _newThisMonth;

    [ObservableProperty]
    private int _activeCustomers;

    [ObservableProperty]
    private int _totalOrders;

    // Pagination
    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    public int TotalPages => TotalCustomers > 0 ? (int)Math.Ceiling((double)TotalCustomers / PageSize) : 1;
    public int PageStart => TotalCustomers > 0 ? (CurrentPage - 1) * PageSize + 1 : 0;
    public int PageEnd => Math.Min(CurrentPage * PageSize, TotalCustomers);
    public bool CanGoPrevious => CurrentPage > 1;
    public bool CanGoNext => CurrentPage < TotalPages;

    public ObservableCollection<CustomerViewModel> Customers { get; } = new();
    public ObservableCollection<CustomerViewModel> FilteredCustomers { get; } = new();
    public ObservableCollection<PageButtonModel> PageNumbers { get; } = new();

    private List<CustomerViewModel> _allCustomers = new();

    public CustomersViewModel()
    {
        LoadMockData();
        UpdateFilteredCustomers();
    }

    private void LoadMockData()
    {
        _allCustomers = new List<CustomerViewModel>
        {
            new() { Id = 1, Name = "Sarah Smith", Email = "sarah@example.com", Phone = "+1 234 567 890", Address = "123 Main St, New York", CreatedAt = DateTime.Now.AddDays(-5) },
            new() { Id = 2, Name = "Michael Brown", Email = "michael.b@tech.com", Phone = "+1 345 678 901", Address = "456 Oak Ave, Los Angeles", CreatedAt = DateTime.Now.AddDays(-10) },
            new() { Id = 3, Name = "Emily Davis", Email = "emily.design@studio.io", Phone = "+1 456 789 012", Address = "789 Pine Rd, Chicago", CreatedAt = DateTime.Now.AddDays(-15) },
            new() { Id = 4, Name = "James Wilson", Email = "jwilson@corp.net", Phone = "+1 567 890 123", Address = "321 Elm St, Houston", CreatedAt = DateTime.Now.AddDays(-20) },
            new() { Id = 5, Name = "Anna Johnson", Email = "anna.j@mail.com", Phone = "+1 678 901 234", Address = "654 Maple Dr, Phoenix", CreatedAt = DateTime.Now.AddDays(-25) },
            new() { Id = 6, Name = "Robert Lee", Email = "robert.lee@business.com", Phone = "+1 789 012 345", Address = "987 Cedar Ln, Philadelphia", CreatedAt = DateTime.Now.AddDays(-30) },
            new() { Id = 7, Name = "Lisa Chen", Email = "lisa.chen@shop.com", Phone = "+1 890 123 456", Address = "147 Birch Way, San Antonio", CreatedAt = DateTime.Now.AddDays(-35) },
            new() { Id = 8, Name = "David Kim", Email = "david.k@email.org", Phone = "+1 901 234 567", Address = "258 Walnut Ct, San Diego", CreatedAt = DateTime.Now.AddDays(-40) },
            new() { Id = 9, Name = "Jennifer White", Email = "j.white@company.com", Phone = "+1 012 345 678", Address = "369 Cherry Blvd, Dallas", CreatedAt = DateTime.Now.AddDays(-45) },
            new() { Id = 10, Name = "Thomas Garcia", Email = "t.garcia@mail.com", Phone = "+1 123 456 789", Address = "741 Spruce St, San Jose", CreatedAt = DateTime.Now.AddDays(-50) },
            new() { Id = 11, Name = "Patricia Martinez", Email = "p.martinez@work.com", Phone = "+1 234 567 891", Address = "852 Ash Ave, Austin", CreatedAt = DateTime.Now.AddDays(-55) },
            new() { Id = 12, Name = "Christopher Lee", Email = "c.lee@tech.io", Phone = "+1 345 678 912", Address = "963 Willow Rd, Jacksonville", CreatedAt = DateTime.Now.AddDays(-60) },
            new() { Id = 13, Name = "Amanda Taylor", Email = "a.taylor@shop.com", Phone = "+1 456 789 123", Address = "159 Hickory Dr, Fort Worth", CreatedAt = DateTime.Now.AddDays(-2) },
            new() { Id = 14, Name = "Daniel Anderson", Email = "d.anderson@corp.net", Phone = "+1 567 891 234", Address = "357 Sycamore Ln, Columbus", CreatedAt = DateTime.Now.AddDays(-3) },
            new() { Id = 15, Name = "Michelle Thomas", Email = "m.thomas@email.org", Phone = "+1 678 912 345", Address = "468 Poplar Way, Charlotte", CreatedAt = DateTime.Now.AddDays(-4) },
        };

        // Calculate stats
        TotalCustomers = _allCustomers.Count;
        NewThisMonth = _allCustomers.Count(c => c.CreatedAt >= DateTime.Now.AddDays(-30));
        ActiveCustomers = _allCustomers.Count(c => c.CreatedAt >= DateTime.Now.AddDays(-60));
        TotalOrders = 47; // Mock value
    }

    partial void OnSearchQueryChanged(string value) => UpdateFilteredCustomers();

    private void UpdateFilteredCustomers()
    {
        var filtered = _allCustomers.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            filtered = filtered.Where(c =>
                c.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                (c.Email?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.Phone?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        var filteredList = filtered.OrderByDescending(c => c.CreatedAt).ToList();
        TotalCustomers = filteredList.Count;

        if (CurrentPage > TotalPages)
        {
            CurrentPage = 1;
        }

        ApplyPagination(filteredList);
    }

    private void ApplyPagination(List<CustomerViewModel> filteredList)
    {
        var pagedCustomers = filteredList
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        FilteredCustomers.Clear();
        foreach (var customer in pagedCustomers)
        {
            FilteredCustomers.Add(customer);
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
            UpdateFilteredCustomers();
        }
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CanGoPrevious)
        {
            CurrentPage--;
            UpdateFilteredCustomers();
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CanGoNext)
        {
            CurrentPage++;
            UpdateFilteredCustomers();
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        LoadMockData();
        UpdateFilteredCustomers();
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

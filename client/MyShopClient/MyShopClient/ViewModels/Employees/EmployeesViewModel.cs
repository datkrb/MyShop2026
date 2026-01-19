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

public partial class EmployeesViewModel : ViewModelBase
{
    private readonly EmployeeApiService _employeeApiService;
    private readonly AppSettingsService _appSettingsService;

    [ObservableProperty]
    private ObservableCollection<EmployeeViewModel> _employees = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    // Stats
    [ObservableProperty]
    private int _totalEmployees;

    [ObservableProperty]
    private int _adminCount;

    [ObservableProperty]
    private int _saleCount;

    // Search
    [ObservableProperty]
    private string _searchQuery = string.Empty;

    // Pagination
    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private int _totalPages = 1;

    public int PageStart => TotalEmployees > 0 ? (CurrentPage - 1) * PageSize + 1 : 0;
    public int PageEnd => Math.Min(CurrentPage * PageSize, TotalEmployees);
    public bool CanGoPrevious => CurrentPage > 1;
    public bool CanGoNext => CurrentPage < TotalPages;

    public ObservableCollection<PageButtonModel> PageNumbers { get; } = new();

    // Debounce timer for search
    private System.Threading.CancellationTokenSource? _searchDebounceToken;

    public EmployeesViewModel(EmployeeApiService employeeApiService, AppSettingsService appSettingsService)
    {
        _employeeApiService = employeeApiService;
        _appSettingsService = appSettingsService;
        _pageSize = _appSettingsService.GetPageSize();
    }

    [RelayCommand]
    public async Task LoadEmployeesAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _employeeApiService.GetPagedAsync(
                page: CurrentPage,
                size: PageSize,
                search: string.IsNullOrWhiteSpace(SearchQuery) ? null : SearchQuery
            );

            if (result != null)
            {
                TotalEmployees = result.Total;
                TotalPages = result.TotalPages > 0 ? result.TotalPages : 1;

                Employees.Clear();
                foreach (var emp in result.Data)
                {
                    Employees.Add(new EmployeeViewModel(emp));
                }

                // Calculate counts from current page data or use stats endpoint if available
                AdminCount = Employees.Count(e => e.Role == "ADMIN");
                SaleCount = Employees.Count(e => e.Role == "SALE");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load employees: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error loading employees: {ex}");
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
                await LoadEmployeesAsync();
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
            await LoadEmployeesAsync();
        }
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CanGoPrevious)
        {
            CurrentPage--;
            await LoadEmployeesAsync();
        }
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CanGoNext)
        {
            CurrentPage++;
            await LoadEmployeesAsync();
        }
    }

    [RelayCommand]
    public async Task DeleteEmployeeAsync(int employeeId)
    {
        try
        {
            var success = await _employeeApiService.DeleteAsync(employeeId);
            if (success)
            {
                await LoadEmployeesAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete employee: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error deleting employee: {ex}");
        }
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        CurrentPage = 1;
        await LoadEmployeesAsync();
    }
}

/// <summary>
/// Employee ViewModel for display in list
/// </summary>
public partial class EmployeeViewModel : ObservableObject
{
    public int Id { get; }
    public string Username { get; }
    public string Role { get; }
    public DateTime? CreatedAt { get; }

    public string FormattedCreatedDate => CreatedAt?.ToString("dd/MM/yyyy") ?? "-";
    
    public string RoleDisplay => Role switch
    {
        "ADMIN" => "Administrator",
        "SALE" => "Sales Staff",
        _ => Role
    };

    public string RoleBadgeColor => Role switch
    {
        "ADMIN" => "#7C5CFC",
        "SALE" => "#10B981",
        _ => "#6B7280"
    };

    public EmployeeViewModel(User user)
    {
        Id = user.Id;
        Username = user.Username;
        Role = user.Role;
        CreatedAt = user.CreatedAt;
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.Models;
using MyShopClient.Services.Api;
using MyShopClient.Services.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyShopClient.ViewModels;

public partial class PromotionViewModel : ObservableObject
{
    private readonly PromotionApiService _promotionService;
    private readonly AppSettingsService _appSettingsService;

    // List
    public ObservableCollection<Promotion> Promotions { get; } = new();
    public ObservableCollection<Promotion> FilteredPromotions { get; } = new();

    [ObservableProperty]
    private Promotion? _selectedPromotion;

    [ObservableProperty]
    private bool _isLoading;

    // Stats
    [ObservableProperty]
    private int _activePromotionsCount;

    [ObservableProperty]
    private int _totalPromotionsCount;

    [ObservableProperty]
    private int _expiredPromotionsCount;

    // Filters
    [ObservableProperty]
    private string _searchKeyword = string.Empty;

    [ObservableProperty]
    private string _selectedStatusFilter = "All";

    [ObservableProperty]
    private string _selectedTypeFilter = "All";

    [ObservableProperty]
    private string _selectedSortOption = "Newest";

    // Filter Options
    public List<string> StatusOptions { get; } = new() { "All", "Active", "Inactive", "Expired" };
    public List<string> TypeOptions { get; } = new() { "All", "PERCENTAGE", "FIXED" };
    public List<string> SortOptions { get; } = new() { "Newest", "Oldest", "Code A-Z", "Code Z-A", "Highest Discount", "Lowest Discount" };

    // Pagination
    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private int _totalPages = 1;

    public int PageStart => TotalPromotionsCount > 0 ? (CurrentPage - 1) * PageSize + 1 : 0;
    public int PageEnd => Math.Min(CurrentPage * PageSize, TotalPromotionsCount);
    public bool CanGoPrevious => CurrentPage > 1;
    public bool CanGoNext => CurrentPage < TotalPages;

    public ObservableCollection<PageButtonModel> PageNumbers { get; } = new();

    // Edit/Create State
    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _editDialogTitle = "Create Promotion";

    [ObservableProperty]
    private Promotion _editingPromotion = new();

    // Dialog visibility control (binding to UI)
    [ObservableProperty]
    private bool _isDialogOpen;

    // Debounce timer for search
    private System.Threading.CancellationTokenSource? _searchDebounceToken;

    public PromotionViewModel(PromotionApiService promotionService, AppSettingsService appSettingsService)
    {
        _promotionService = promotionService;
        _appSettingsService = appSettingsService;
        _pageSize = _appSettingsService.GetPageSize();
    }

    private void UpdateStats()
    {
        var now = DateTime.Now;
        // Note: Stats are calculated from current page data, for accurate total stats consider adding a stats endpoint
        ActivePromotionsCount = Promotions.Count(p => p.IsActive && p.StartDate <= now && p.EndDate >= now);
        ExpiredPromotionsCount = Promotions.Count(p => p.EndDate < now);
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

    partial void OnSearchKeywordChanged(string value)
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
                await LoadPromotions();
            }
        }
        catch (TaskCanceledException)
        {
            // Ignored - expected when user types again before delay completes
        }
    }

    partial void OnSelectedStatusFilterChanged(string value)
    {
        CurrentPage = 1;
        ApplyFilters();
    }

    partial void OnSelectedTypeFilterChanged(string value)
    {
        CurrentPage = 1;
        ApplyFilters();
    }

    partial void OnSelectedSortOptionChanged(string value)
    {
        ApplyFilters();
    }

    [RelayCommand]
    private void ApplyFilters()
    {
        var now = DateTime.Now;
        var filtered = Promotions.AsEnumerable();

        // Status filter
        if (SelectedStatusFilter != "All")
        {
            filtered = SelectedStatusFilter switch
            {
                "Active" => filtered.Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now),
                "Inactive" => filtered.Where(p => !p.IsActive),
                "Expired" => filtered.Where(p => p.EndDate < now),
                _ => filtered
            };
        }

        // Type filter
        if (SelectedTypeFilter != "All")
        {
            filtered = filtered.Where(p => p.DiscountType == SelectedTypeFilter);
        }

        // Sort
        filtered = SelectedSortOption switch
        {
            "Newest" => filtered.OrderByDescending(p => p.StartDate),
            "Oldest" => filtered.OrderBy(p => p.StartDate),
            "Code A-Z" => filtered.OrderBy(p => p.Code),
            "Code Z-A" => filtered.OrderByDescending(p => p.Code),
            "Highest Discount" => filtered.OrderByDescending(p => p.DiscountValue),
            "Lowest Discount" => filtered.OrderBy(p => p.DiscountValue),
            _ => filtered.OrderByDescending(p => p.StartDate)
        };

        FilteredPromotions.Clear();
        foreach (var item in filtered)
        {
            FilteredPromotions.Add(item);
        }
    }

    [RelayCommand]
    public async Task LoadPromotions()
    {
        IsLoading = true;
        try
        {
            var result = await _promotionService.GetPromotionsAsync(
                page: CurrentPage, 
                size: PageSize, 
                search: string.IsNullOrWhiteSpace(SearchKeyword) ? null : SearchKeyword
            );
            
            Promotions.Clear();
            if (result?.Data != null)
            {
                TotalPromotionsCount = result.Total;
                TotalPages = result.TotalPages > 0 ? result.TotalPages : 1;

                foreach (var item in result.Data)
                {
                    Promotions.Add(item);
                }
            }
            UpdateStats();
            ApplyFilters();
        }
        catch (Exception ex)
        {
            // Handle error
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally
        {
            IsLoading = false;
            UpdatePaginationProperties();
            UpdatePageNumbers();
        }
    }

    [RelayCommand]
    public async Task GoToPageAsync(int pageNumber)
    {
        if (pageNumber >= 1 && pageNumber <= TotalPages && pageNumber != CurrentPage)
        {
            CurrentPage = pageNumber;
            await LoadPromotions();
        }
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CanGoPrevious)
        {
            CurrentPage--;
            await LoadPromotions();
        }
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CanGoNext)
        {
            CurrentPage++;
            await LoadPromotions();
        }
    }

    [RelayCommand]
    private void Add()
    {
        EditingPromotion = new Promotion 
        { 
            StartDate = DateTime.Now, 
            EndDate = DateTime.Now.AddDays(7),
            IsActive = true,
            DiscountType = "PERCENTAGE",
            DiscountValue = 10 
        };
        EditDialogTitle = "Create Promotion";
        IsEditing = false;
        IsDialogOpen = true;
    }

    [RelayCommand]
    private void Edit(Promotion? promotion)
    {
        if (promotion == null) return;

        // Clone to avoid editing list item directly before save
        EditingPromotion = new Promotion
        {
            Id = promotion.Id,
            Code = promotion.Code,
            Description = promotion.Description,
            DiscountType = promotion.DiscountType,
            DiscountValue = promotion.DiscountValue,
            MinOrderValue = promotion.MinOrderValue,
            MaxDiscount = promotion.MaxDiscount,
            UsageLimit = promotion.UsageLimit,
            UsedCount = promotion.UsedCount,
            StartDate = promotion.StartDate,
            EndDate = promotion.EndDate,
            IsActive = promotion.IsActive
        };
        EditDialogTitle = "Edit Promotion";
        IsEditing = true;
        IsDialogOpen = true;
    }

    [RelayCommand]
    private async Task Save()
    {
        IsLoading = true;
        try
        {
            if (IsEditing)
            {
                await _promotionService.UpdatePromotionAsync(EditingPromotion.Id, EditingPromotion);
            }
            else
            {
                await _promotionService.CreatePromotionAsync(EditingPromotion);
            }
            
            IsDialogOpen = false;
            await LoadPromotions();
        }
        catch (Exception ex)
        {
            // Handle error
            System.Diagnostics.Debug.WriteLine($"Save failed: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Delete(Promotion? promotion)
    {
        if (promotion == null) return;
        
        IsLoading = true;
        try
        {
            await _promotionService.DeletePromotionAsync(promotion.Id);
            IsDialogOpen = false;
            await LoadPromotions();
        }
        catch (Exception ex)
        {
             System.Diagnostics.Debug.WriteLine($"Delete failed: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private void CancelEdit()
    {
        IsDialogOpen = false;
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchKeyword = string.Empty;
        SelectedStatusFilter = "All";
        SelectedTypeFilter = "All";
        SelectedSortOption = "Newest";
        CurrentPage = 1;
        _ = LoadPromotions();
    }
}

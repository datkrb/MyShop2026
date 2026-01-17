using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.Models;
using MyShopClient.Services.Api;
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

    public PromotionViewModel(PromotionApiService promotionService)
    {
        _promotionService = promotionService;
        LoadPromotionsCommand.Execute(null);
    }

    private void UpdateStats()
    {
        var now = DateTime.Now;
        TotalPromotionsCount = Promotions.Count;
        ActivePromotionsCount = Promotions.Count(p => p.IsActive && p.StartDate <= now && p.EndDate >= now);
        ExpiredPromotionsCount = Promotions.Count(p => p.EndDate < now);
    }

    partial void OnSearchKeywordChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedStatusFilterChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedTypeFilterChanged(string value)
    {
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

        // Search filter
        if (!string.IsNullOrWhiteSpace(SearchKeyword))
        {
            var keyword = SearchKeyword.ToLower();
            filtered = filtered.Where(p => 
                (p.Code?.ToLower().Contains(keyword) ?? false) ||
                (p.Description?.ToLower().Contains(keyword) ?? false));
        }

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
    private async Task LoadPromotions()
    {
        IsLoading = true;
        try
        {
            var result = await _promotionService.GetPromotionsAsync(page: 1, size: 100, search: string.Empty);
            Promotions.Clear();
            if (result?.Data != null)
            {
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
        IsEditing = false; // "IsEditing" here means if we are editing *existing* vs creating? Or generally in edit mode? 
                           // Let's use it as "IsEditMode" for deciding Update vs Create API call.
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
        ApplyFilters();
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShopClient.ViewModels;

/// <summary>
/// ViewModel for Create/Edit Order Dialog
/// </summary>
public partial class CreateOrderDialogViewModel : ObservableObject
{
    // ... existing properties ...

    // Edit mode
    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private int _orderId;

    [ObservableProperty]
    private string _dialogTitle = "Create New Order";

    // Customer info
    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private string _customerEmail = string.Empty;

    [ObservableProperty]
    private string _customerPhone = string.Empty;

    // Order items
    public ObservableCollection<CreateOrderItemViewModel> OrderItems { get; } = new();

    public bool HasNoItems => OrderItems.Count == 0;

    // Available products for selection
    public ObservableCollection<ProductSelectionItem> AvailableProducts { get; } = new();

    [ObservableProperty]
    private ProductSelectionItem? _selectedProduct;

    [ObservableProperty]
    private int _quantity = 1;

    // Order status
    public ObservableCollection<string> Statuses { get; } = new() { "New", "Paid", "Canceled" };

    [ObservableProperty]
    private string _selectedStatus = "New";

    // UI States
    [ObservableProperty]
    private bool _hasErrors;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isSaving;

    // Computed totals

    public decimal Subtotal => OrderItems.Sum(x => x.TotalPrice);
    public decimal Tax => Subtotal * 0.1m;
    public decimal Total => Math.Max(0, Subtotal + Tax - DiscountAmount);

    public string FormattedSubtotal => Helpers.CurrencyHelper.FormatVND(Subtotal);
    public string FormattedTax => Helpers.CurrencyHelper.FormatVND(Tax);
    public string FormattedTotal => Helpers.CurrencyHelper.FormatVND(Total);

    // Services
    private readonly Services.Api.PromotionApiService? _promotionService;

    // Promotion
    [ObservableProperty]
    private string _promotionCode = string.Empty;

    [ObservableProperty]
    private decimal _discountAmount;

    [ObservableProperty]
    private string _promotionMessage = string.Empty;
    
    [ObservableProperty]
    private bool _isPromotionValid;

    [ObservableProperty]
    private int? _appliedPromotionId;

    // Events
    public event EventHandler<bool>? DialogCloseRequested;

    // Design-time constructor
    public CreateOrderDialogViewModel()
    {
        // Load mock available products
        AvailableProducts.Add(new ProductSelectionItem { Id = 1, Name = "iPhone 15 Pro", Price = 999.00m, ImageUrl = "https://ui-avatars.com/api/?name=IP&background=7C5CFC&color=fff" });
        AvailableProducts.Add(new ProductSelectionItem { Id = 2, Name = "MacBook Pro 14\"", Price = 1999.00m, ImageUrl = "https://ui-avatars.com/api/?name=MB&background=10B981&color=fff" });
        AvailableProducts.Add(new ProductSelectionItem { Id = 3, Name = "AirPods Pro", Price = 249.00m, ImageUrl = "https://ui-avatars.com/api/?name=AP&background=3B82F6&color=fff" });
        AvailableProducts.Add(new ProductSelectionItem { Id = 4, Name = "iPad Pro 12.9\"", Price = 1099.00m, ImageUrl = "https://ui-avatars.com/api/?name=PD&background=F59E0B&color=fff" });
        AvailableProducts.Add(new ProductSelectionItem { Id = 5, Name = "Apple Watch Ultra", Price = 799.00m, ImageUrl = "https://ui-avatars.com/api/?name=AW&background=EF4444&color=fff" });
    }

    // DI Constructor
    [Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor]
    public CreateOrderDialogViewModel(Services.Api.PromotionApiService promotionService) : this()
    {
        _promotionService = promotionService;
    }

    /// <summary>
    /// Load existing order data for editing
    /// </summary>
    public void LoadOrder(OrderViewModel order)
    {
        IsEditMode = true;
        DialogTitle = $"Edit Order {order.OrderId}";
        OrderId = order.Id;
        CustomerName = order.CustomerName;
        CustomerEmail = order.CustomerEmail;
        SelectedStatus = order.OrderStatus;

        // Clear and add order items (mock data for now)
        OrderItems.Clear();
        OrderItems.Add(new CreateOrderItemViewModel
        {
            ProductId = 1,
            ProductName = "Product Item 1",
            ProductImage = "https://ui-avatars.com/api/?name=P1&background=7C5CFC&color=fff",
            Quantity = 2,
            UnitPrice = order.Amount * 0.4m,
            TotalPrice = order.Amount * 0.4m * 2
        });
        OrderItems.Add(new CreateOrderItemViewModel
        {
            ProductId = 2,
            ProductName = "Product Item 2",
            ProductImage = "https://ui-avatars.com/api/?name=P2&background=10B981&color=fff",
            Quantity = 1,
            UnitPrice = order.Amount * 0.2m,
            TotalPrice = order.Amount * 0.2m
        });

        NotifyTotalsChanged();
    }

    /// <summary>
    /// Load from OrderDetailViewModel for editing
    /// </summary>
    public void LoadFromDetailViewModel(OrderDetailViewModel detail)
    {
        IsEditMode = true;
        DialogTitle = $"Edit Order {detail.OrderId}";
        OrderId = detail.Id;
        CustomerName = detail.CustomerName;
        CustomerEmail = detail.CustomerEmail;
        SelectedStatus = detail.OrderStatus;

        // Clear and add order items from detail
        OrderItems.Clear();
        foreach (var item in detail.OrderItems)
        {
            OrderItems.Add(new CreateOrderItemViewModel
            {
                ProductId = 0, // Not available from OrderItemViewModel
                ProductName = item.ProductName,
                ProductImage = item.ProductImage,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice
            });
        }

        NotifyTotalsChanged();
    }

    private void NotifyTotalsChanged()
    {
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(Tax));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(FormattedSubtotal));
        OnPropertyChanged(nameof(FormattedTax));
        OnPropertyChanged(nameof(FormattedTotal));
        OnPropertyChanged(nameof(FormattedDiscount));

        // Re-validate promotion if applied
        if (IsPromotionValid && !string.IsNullOrEmpty(PromotionCode))
        {
            ApplyPromotionCommand.Execute(null);
        }
    }

    public string FormattedDiscount => $"-{Helpers.CurrencyHelper.FormatVND(DiscountAmount)}";

    [RelayCommand]
    private async Task ApplyPromotion()
    {
        if (string.IsNullOrWhiteSpace(PromotionCode))
        {
            DiscountAmount = 0;
            IsPromotionValid = false;
            PromotionMessage = string.Empty;
            AppliedPromotionId = null;
            NotifyTotalsChanged(); // Avoid infinite loop by only calling if discount changed
            return;
        }

        if (_promotionService == null)
        {
            PromotionMessage = "Service not available";
            return;
        }

        var result = await _promotionService.ValidatePromotionAsync(PromotionCode, Subtotal);
        if (result != null && result.Valid)
        {
            DiscountAmount = result.DiscountAmount ?? 0;
            IsPromotionValid = true;
            AppliedPromotionId = result.Promotion?.Id;
            PromotionMessage = $"Applied: {result.Promotion?.Code} (-${DiscountAmount:N0})";
            // Manually notify total changed without triggering recursive check
            OnPropertyChanged(nameof(Total)); 
            OnPropertyChanged(nameof(FormattedTotal));
            OnPropertyChanged(nameof(FormattedDiscount));
        }
        else
        {
            DiscountAmount = 0;
            IsPromotionValid = false;
            AppliedPromotionId = null;
            PromotionMessage = result?.Message ?? "Invalid code";
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(FormattedTotal));
            OnPropertyChanged(nameof(FormattedDiscount));
        }
    }

    [RelayCommand]
    private void AddItem()
    {
        if (SelectedProduct == null || Quantity <= 0) return;

        // Check if item already exists
        var existingItem = OrderItems.FirstOrDefault(x => x.ProductId == SelectedProduct.Id);
        if (existingItem != null)
        {
            existingItem.Quantity += Quantity;
            existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice;
        }
        else
        {
            OrderItems.Add(new CreateOrderItemViewModel
            {
                ProductId = SelectedProduct.Id,
                ProductName = SelectedProduct.Name,
                ProductImage = SelectedProduct.ImageUrl,
                Quantity = Quantity,
                UnitPrice = SelectedProduct.Price,
                TotalPrice = Quantity * SelectedProduct.Price
            });
        }

        // Reset selection
        SelectedProduct = null;
        Quantity = 1;

        // Notify totals and empty state changed
        NotifyTotalsChanged();
        OnPropertyChanged(nameof(HasNoItems));
    }

    [RelayCommand]
    private void RemoveItem(CreateOrderItemViewModel? item)
    {
        if (item == null) return;

        OrderItems.Remove(item);

        // Notify totals and empty state changed
        NotifyTotalsChanged();
        OnPropertyChanged(nameof(HasNoItems));
    }

    [RelayCommand]
    private async Task Save()
    {
        // Validate
        if (string.IsNullOrWhiteSpace(CustomerName))
        {
            HasErrors = true;
            ErrorMessage = "Customer name is required";
            return;
        }

        if (OrderItems.Count == 0)
        {
            HasErrors = true;
            ErrorMessage = "Please add at least one item to the order";
            return;
        }

        HasErrors = false;
        ErrorMessage = string.Empty;
        IsSaving = true;

        // TODO: Save order to API
        // NOTE: In real implementation, include AppliedPromotionId and PromotionCode in the API DTO
        await Task.Delay(500); // Simulate API call

        IsSaving = false;
        DialogCloseRequested?.Invoke(this, true);
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogCloseRequested?.Invoke(this, false);
    }
}

/// <summary>
/// Product item for selection dropdown
/// </summary>
public class ProductSelectionItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;

    public override string ToString() => $"{Name} - {Helpers.CurrencyHelper.FormatVND(Price)}";
}

/// <summary>
/// Order item for Create Order dialog
/// </summary>
public partial class CreateOrderItemViewModel : ObservableObject
{
    public int ProductId { get; set; }

    [ObservableProperty]
    private string _productName = string.Empty;

    [ObservableProperty]
    private string _productImage = string.Empty;

    [ObservableProperty]
    private int _quantity;

    [ObservableProperty]
    private decimal _unitPrice;

    [ObservableProperty]
    private decimal _totalPrice;

    public string FormattedUnitPrice => Helpers.CurrencyHelper.FormatVND(UnitPrice);
    public string FormattedTotalPrice => Helpers.CurrencyHelper.FormatVND(TotalPrice);
}

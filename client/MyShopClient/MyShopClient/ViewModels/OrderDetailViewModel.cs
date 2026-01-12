using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.Models;
using MyShopClient.Services.Api;
using MyShopClient.Views.Orders;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace MyShopClient.ViewModels;

/// <summary>
/// ViewModel for Order Detail page (View, Edit, Create)
/// </summary>
public partial class OrderDetailViewModel : ObservableObject
{
    private readonly OrderApiService _orderApiService; // Assumption: we might need this later
    // In current code, it wasn't injected. I'll stick to non-DI for service or simple usage if possible, 
    // but better to add it if I'm doing real save.
    // For now, I'll focus on the UI properties.

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isNewOrder;

    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _orderId = string.Empty;

    // Customer Info
    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private string _customerEmail = string.Empty;
    
    [ObservableProperty]
    private string _customerPhone = string.Empty;

    [ObservableProperty]
    private string _customerAddress = string.Empty;

    [ObservableProperty]
    private string _customerAvatar = string.Empty;
    
    [ObservableProperty]
    private int? _customerId;

    [ObservableProperty]
    private DateTime _orderDate;

    [ObservableProperty]
    private decimal _amount;

    [ObservableProperty]
    private string _orderStatus = "Pending";

    [ObservableProperty]
    private string _orderStatusBackground = "#F3F4F6";

    [ObservableProperty]
    private string _orderStatusForeground = "#4B5563";
    
    public ObservableCollection<string> Statuses { get; } = new() 
    { 
        "Pending", "Processing", "Shipped", "Delivered", "Cancelled" 
    };

    [ObservableProperty]
    private string _selectedStatus = "Pending";

    // Order Items
    public ObservableCollection<OrderItemViewModel> OrderItems { get; } = new();

    // Computed properties
    public string FormattedDate => OrderDate.ToString("MMM dd, yyyy 'at' HH:mm");
    public string FormattedAmount => $"${Amount:N2}";
    
    public decimal Subtotal => OrderItems.Sum(x => x.TotalPrice);
    public decimal Tax => Subtotal * 0.1m; // 10% tax
    public decimal Total => Subtotal + Tax;
    
    public string FormattedSubtotal => $"${Subtotal:N2}";
    public string FormattedTax => $"${Tax:N2}";
    public string FormattedTotal => $"${Total:N2}";

    public OrderDetailViewModel()
    {
        // Default constructor
        OrderDate = DateTime.Now;
    }

    public void LoadOrder(OrderViewModel order)
    {
        IsNewOrder = false;
        IsEditing = false;
        
        Id = order.Id;
        OrderId = order.OrderId;
        
        CustomerId = 0; // Info not strictly in OrderViewModel, need real object. 
        // For Refactor: We should ideally load full details from API.
        // But adapting to existing:
        CustomerName = order.CustomerName;
        CustomerEmail = order.CustomerEmail;
        CustomerAvatar = order.CustomerAvatar;
        
        OrderDate = order.OrderDate;
        Amount = order.Amount; // In edit mode, this will be recalculated
        OrderStatus = order.OrderStatus;
        SelectedStatus = OrderStatus;
        
        OrderStatusBackground = order.OrderStatusBackground;
        OrderStatusForeground = order.OrderStatusForeground;

        // Load items (Mock for now as per previous)
        OrderItems.Clear();
        // Preserving existing logic or loading real items if available
        // Existing logic added mock items manually.
        OrderItems.Add(new OrderItemViewModel 
        { 
            ProductName = "Product Item 1", 
            ProductImage = "https://ui-avatars.com/api/?name=P1&background=7C5CFC&color=fff",
            Quantity = 2, 
            UnitPrice = Amount * 0.4m,
            TotalPrice = Amount * 0.4m * 2
        });
        
        RecalculateTotals();
    }
    
    public void InitializeNewOrder()
    {
        IsNewOrder = true;
        IsEditing = true;
        
        OrderId = "NEW";
        OrderDate = DateTime.Now;
        OrderStatus = "Pending";
        SelectedStatus = "Pending";
        
        CustomerName = "Select Customer";
        CustomerEmail = "";
        CustomerAvatar = "";
        
        OrderItems.Clear();
        RecalculateTotals();
    }
    
    public void EnableEdit()
    {
        IsEditing = true;
    }

    private void RecalculateTotals()
    {
        Amount = Subtotal; // Base amount
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(Tax));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(FormattedSubtotal));
        OnPropertyChanged(nameof(FormattedTax));
        OnPropertyChanged(nameof(FormattedTotal));
    }

    [RelayCommand]
    private async Task SelectCustomer()
    {
        var vm = new CustomerSelectionViewModel(CustomerApiService.Instance);
        var dialog = new CustomerSelectionDialog
        {
            XamlRoot = App.Current.MainWindow.Content.XamlRoot,
            ViewModel = vm
        };
        
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && vm.SelectedCustomer != null)
        {
            var customer = vm.SelectedCustomer;
            CustomerId = customer.Id;
            CustomerName = customer.Name;
            CustomerEmail = customer.Email ?? "";
            CustomerPhone = customer.Phone ?? "";
            CustomerAddress = customer.Address ?? "";
            CustomerAvatar = customer.AvatarUrl;
        }
    }

    [RelayCommand]
    private async Task AddProduct()
    {
        var vm = new ProductSelectionViewModel(ProductApiService.Instance);
        var dialog = new ProductSelectionDialog
        {
            XamlRoot = App.Current.MainWindow.Content.XamlRoot,
            ViewModel = vm
        };
        
        var result = await dialog.ShowAsync();
        // If dialog result is None but we handled "Add" internally, we might need a mechanism.
        // Actually, simpler: Dialog has "Add" button that closes it or returning selected product.
        // Let's assume Primary Button is "Select"
        
        if (result == ContentDialogResult.Primary && vm.SelectedProduct != null)
        {
            var product = vm.SelectedProduct;
            var existing = OrderItems.FirstOrDefault(i => i.ProductName == product.Name); 
            // Better to match by ID, but OrderItemViewModel doesn't have ID yet.
            // I should add ProductId to OrderItemViewModel.
            
            if (existing != null)
            {
                existing.Quantity++;
                existing.TotalPrice = existing.Quantity * existing.UnitPrice;
            }
            else
            {
                OrderItems.Add(new OrderItemViewModel
                {
                    ProductName = product.Name,
                    ProductImage = (product.Images != null && product.Images.Count > 0) ? product.Images[0].Url : "",
                    Quantity = 1,
                    UnitPrice = product.SalePrice,
                    TotalPrice = product.SalePrice
                });
            }
            RecalculateTotals();
        }
    }

    [RelayCommand]
    private void RemoveItem(OrderItemViewModel item)
    {
        OrderItems.Remove(item);
        RecalculateTotals();
    }
    
    [RelayCommand]
    private void Save()
    {
        // TODO: Implement Save via API
        // For now, just exit edit mode?
        // Or if New, create.
        
        IsEditing = false;
        IsNewOrder = false;
        OrderStatus = SelectedStatus;
    }

    [RelayCommand]
    private void Cancel()
    {
        if (IsNewOrder)
        {
            // Go back
             if (App.Current.ContentFrame.CanGoBack)
                App.Current.ContentFrame.GoBack();
        }
        else
        {
            IsEditing = false;
            // Reload original state... 
        }
    }

    [RelayCommand]
    private void EditStatus()
    {
       EnableEdit();
    }

    [RelayCommand]
    private void Print()
    {
        // TODO: Implement print order
    }

    [RelayCommand]
    private void DeleteOrder()
    {
        // TODO: Implement delete order
    }
}

/// <summary>
/// ViewModel for Order Item display in Order Detail
/// </summary>
public partial class OrderItemViewModel : ObservableObject
{
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

    public string FormattedUnitPrice => $"${UnitPrice:N2}";
    public string FormattedTotalPrice => $"${TotalPrice:N2}";
    
    partial void OnQuantityChanged(int value)
    {
        TotalPrice = UnitPrice * value;
    }
}

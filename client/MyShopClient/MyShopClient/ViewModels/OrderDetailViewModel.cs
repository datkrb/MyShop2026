using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;

namespace MyShopClient.ViewModels;

/// <summary>
/// ViewModel for Order Detail page
/// </summary>
public partial class OrderDetailViewModel : ObservableObject
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
    private string _orderStatusBackground = "#F3F4F6";

    [ObservableProperty]
    private string _orderStatusForeground = "#4B5563";

    // Order Items
    public ObservableCollection<OrderItemViewModel> OrderItems { get; } = new();

    // Computed properties
    public string FormattedDate => OrderDate.ToString("MMM dd, yyyy 'at' HH:mm");
    public string FormattedAmount => $"${Amount:N2}";
    
    public decimal Subtotal => Amount;
    public decimal Tax => Amount * 0.1m; // 10% tax
    public decimal Total => Subtotal + Tax;
    
    public string FormattedSubtotal => $"${Subtotal:N2}";
    public string FormattedTax => $"${Tax:N2}";
    public string FormattedTotal => $"${Total:N2}";

    public void LoadOrder(OrderViewModel order)
    {
        Id = order.Id;
        OrderId = order.OrderId;
        CustomerName = order.CustomerName;
        CustomerEmail = order.CustomerEmail;
        CustomerAvatar = order.CustomerAvatar;
        OrderDate = order.OrderDate;
        Amount = order.Amount;
        OrderStatus = order.OrderStatus;
        OrderStatusBackground = order.OrderStatusBackground;
        OrderStatusForeground = order.OrderStatusForeground;

        // Load mock order items
        OrderItems.Clear();
        OrderItems.Add(new OrderItemViewModel 
        { 
            ProductName = "Product Item 1", 
            ProductImage = "https://ui-avatars.com/api/?name=P1&background=7C5CFC&color=fff",
            Quantity = 2, 
            UnitPrice = Amount * 0.4m,
            TotalPrice = Amount * 0.4m * 2
        });
        OrderItems.Add(new OrderItemViewModel 
        { 
            ProductName = "Product Item 2",
            ProductImage = "https://ui-avatars.com/api/?name=P2&background=10B981&color=fff",
            Quantity = 1, 
            UnitPrice = Amount * 0.2m,
            TotalPrice = Amount * 0.2m
        });
    }

    [RelayCommand]
    private void EditStatus()
    {
        // TODO: Implement edit status
    }

    [RelayCommand]
    private void Print()
    {
        // TODO: Implement print order
    }

    [RelayCommand]
    private void CancelOrder()
    {
        // TODO: Implement cancel order
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
}

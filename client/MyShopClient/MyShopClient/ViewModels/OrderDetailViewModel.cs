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
    private readonly OrderApiService _orderApiService;

    [ObservableProperty]
    private bool _isLoading;

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
    private string _orderStatus = "DRAFT";

    [ObservableProperty]
    private string _createdByUsername = string.Empty;
    
    public ObservableCollection<string> Statuses { get; } = new() 
    { 
        "DRAFT", "PENDING", "PAID", "CANCELLED" 
    };

    [ObservableProperty]
    private string _selectedStatus = "DRAFT";

    // Order Items
    public ObservableCollection<OrderItemViewModel> OrderItems { get; } = new();

    // Computed properties
    public string FormattedDate => OrderDate.ToString("dd/MM/yyyy HH:mm");
    public string FormattedAmount => $"{Amount:N0}đ";
    
    public decimal Subtotal => OrderItems.Sum(x => x.TotalPrice);
    public decimal Total => Subtotal; // No tax calculation for simplicity
    
    public string FormattedSubtotal => $"{Subtotal:N0}đ";
    public string FormattedTotal => $"{Total:N0}đ";

    public string DisplayStatus => OrderStatus switch
    {
        "DRAFT" => "Draft",
        "PENDING" => "Pending",
        "PAID" => "Paid",
        "CANCELLED" => "Cancelled",
        _ => OrderStatus
    };

    public string StatusBackground => OrderStatus switch
    {
        "DRAFT" => "#F3F4F6",
        "PENDING" => "#FEF3C7",
        "PAID" => "#DCFCE7",
        "CANCELLED" => "#FEE2E2",
        _ => "#F3F4F6"
    };

    public string StatusForeground => OrderStatus switch
    {
        "DRAFT" => "#4B5563",
        "PENDING" => "#CA8A04",
        "PAID" => "#15803D",
        "CANCELLED" => "#B91C1C",
        _ => "#4B5563"
    };

    public OrderDetailViewModel()
    {
        _orderApiService = OrderApiService.Instance;
        OrderDate = DateTime.Now;
    }

    /// <summary>
    /// Load order from API by ID
    /// </summary>
    public async Task LoadOrderAsync(int orderId)
    {
        IsLoading = true;
        IsNewOrder = false;
        IsEditing = false;

        try
        {
            var order = await _orderApiService.GetOrderAsync(orderId);

            if (order != null)
            {
                Id = order.Id;
                OrderId = $"#ORD-{order.Id:D4}";
                OrderStatus = order.Status;
                SelectedStatus = order.Status;
                OrderDate = order.CreatedTime;
                Amount = order.FinalPrice;

                // Customer info
                CustomerId = order.CustomerId;
                CustomerName = order.Customer?.Name ?? "Walk-in Customer";
                CustomerEmail = order.Customer?.Email ?? "";
                CustomerPhone = order.Customer?.Phone ?? "";
                CustomerAddress = order.Customer?.Address ?? "";
                CustomerAvatar = order.Customer?.AvatarUrl ?? $"https://ui-avatars.com/api/?name=Guest&background=7C5CFC&color=fff";

                // Created by
                CreatedByUsername = order.CreatedBy?.Username ?? "";

                // Order items
                OrderItems.Clear();
                if (order.OrderItems != null)
                {
                    foreach (var item in order.OrderItems)
                    {
                        OrderItems.Add(new OrderItemViewModel
                        {
                            ProductId = item.ProductId,
                            ProductName = item.Product?.Name ?? "Unknown Product",
                            ProductSku = item.Product?.Sku ?? "",
                            ProductImage = "", // Would need images from product
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitSalePrice,
                            TotalPrice = item.TotalPrice
                        });
                    }
                }

                RecalculateTotals();
                OnPropertyChanged(nameof(DisplayStatus));
                OnPropertyChanged(nameof(StatusBackground));
                OnPropertyChanged(nameof(StatusForeground));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading order: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Load from OrderViewModel (quick load for navigation)
    /// </summary>
    public void LoadOrder(OrderViewModel order)
    {
        IsNewOrder = false;
        IsEditing = false;
        
        Id = order.Id;
        OrderId = order.OrderId;
        CustomerName = order.CustomerName;
        CustomerEmail = order.CustomerEmail;
        CustomerAvatar = order.CustomerAvatar;
        OrderDate = order.OrderDate;
        Amount = order.Amount;
        OrderStatus = order.OrderStatus;
        SelectedStatus = order.OrderStatus;

        // Load full details from API
        _ = LoadOrderAsync(order.Id);
    }
    
    public void InitializeNewOrder()
    {
        IsNewOrder = true;
        IsEditing = true;
        
        OrderId = "NEW";
        OrderDate = DateTime.Now;
        OrderStatus = "DRAFT";
        SelectedStatus = "DRAFT";
        
        CustomerName = "Select Customer";
        CustomerEmail = "";
        CustomerPhone = "";
        CustomerAddress = "";
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
        Amount = Subtotal;
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(FormattedSubtotal));
        OnPropertyChanged(nameof(FormattedTotal));
        OnPropertyChanged(nameof(FormattedAmount));
    }

    [RelayCommand]
    private async Task SelectCustomer()
    {
        var dialog = new CustomerSelectionDialog
        {
            XamlRoot = App.Current.MainWindow.Content.XamlRoot
        };
        
        // Load customers when dialog opens
        dialog.LoadCustomers();
        
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.ViewModel.SelectedCustomer != null)
        {
            var customer = dialog.ViewModel.SelectedCustomer;
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
        
        if (result == ContentDialogResult.Primary && vm.SelectedProduct != null)
        {
            var product = vm.SelectedProduct;
            var existing = OrderItems.FirstOrDefault(i => i.ProductId == product.Id);
            
            if (existing != null)
            {
                existing.Quantity++;
                existing.TotalPrice = existing.Quantity * existing.UnitPrice;
            }
            else
            {
                OrderItems.Add(new OrderItemViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductSku = product.Sku,
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
    private async Task SaveAsync()
    {
        IsLoading = true;

        try
        {
            if (IsNewOrder)
            {
                var request = new CreateOrderRequest
                {
                    CustomerId = CustomerId,
                    Status = SelectedStatus,
                    Items = OrderItems.Select(i => new CreateOrderItemRequest
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity
                    }).ToList()
                };

                var result = await _orderApiService.CreateOrderAsync(request);
                if (result != null)
                {
                    Id = result.Id;
                    OrderId = $"#ORD-{result.Id:D4}";
                    IsNewOrder = false;
                    IsEditing = false;
                }
            }
            else
            {
                var request = new UpdateOrderRequest
                {
                    CustomerId = CustomerId,
                    Status = SelectedStatus,
                    Items = OrderItems.Select(i => new CreateOrderItemRequest
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity
                    }).ToList()
                };

                var result = await _orderApiService.UpdateOrderAsync(Id, request);
                if (result != null)
                {
                    OrderStatus = result.Status;
                    IsEditing = false;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving order: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        if (IsNewOrder)
        {
            if (App.Current.ContentFrame.CanGoBack)
                App.Current.ContentFrame.GoBack();
        }
        else
        {
            IsEditing = false;
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
    public async Task DeleteOrderAsync()
    {
        try
        {
            var success = await _orderApiService.DeleteOrderAsync(Id);
            if (success && App.Current.ContentFrame.CanGoBack)
            {
                App.Current.ContentFrame.GoBack();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting order: {ex.Message}");
        }
    }
}

/// <summary>
/// ViewModel for Order Item display in Order Detail
/// </summary>
public partial class OrderItemViewModel : ObservableObject
{
    [ObservableProperty]
    private int _productId;

    [ObservableProperty]
    private string _productName = string.Empty;

    [ObservableProperty]
    private string _productSku = string.Empty;

    [ObservableProperty]
    private string _productImage = string.Empty;

    [ObservableProperty]
    private int _quantity;

    [ObservableProperty]
    private decimal _unitPrice;

    [ObservableProperty]
    private decimal _totalPrice;

    public string FormattedUnitPrice => $"{UnitPrice:N0}đ";
    public string FormattedTotalPrice => $"{TotalPrice:N0}đ";
    
    partial void OnQuantityChanged(int value)
    {
        TotalPrice = UnitPrice * value;
        OnPropertyChanged(nameof(FormattedTotalPrice));
    }
}

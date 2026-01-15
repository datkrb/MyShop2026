using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.Models;
using MyShopClient.Services.Api;
using MyShopClient.Services.Navigation;
using MyShopClient.Views.Orders;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace MyShopClient.ViewModels;

/// <summary>
/// ViewModel for Order Detail page (View, Edit, Create)
/// </summary>
public partial class OrderDetailViewModel : ObservableObject
{
    private readonly OrderApiService _orderApiService;
    private readonly INavigationService _navigationService;

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
    [NotifyPropertyChangedFor(nameof(DisplayStatus))]
    [NotifyPropertyChangedFor(nameof(StatusBackground))]
    [NotifyPropertyChangedFor(nameof(StatusForeground))]
    private string _orderStatus = "DRAFT";

    [ObservableProperty]
    private string _createdByUsername = string.Empty;
    
    public ObservableCollection<string> Statuses { get; } = new() 
    { 
        "DRAFT", "PENDING", "PAID", "CANCELLED" 
    };

    [ObservableProperty]
    private string _selectedStatus = "DRAFT";

    // Notification properties
    [ObservableProperty]
    private bool _isInfoBarOpen;

    [ObservableProperty]
    private bool _isTipOpen;

    [ObservableProperty]
    private string _infoBarMessage = string.Empty;

    [ObservableProperty]
    private string _infoBarSeverity = "Informational"; // Success, Warning, Error, Informational

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

    private readonly DispatcherTimer _autoSaveTimer;

    public OrderDetailViewModel(OrderApiService orderApiService, INavigationService navigationService)
    {
        _orderApiService = orderApiService ?? throw new ArgumentNullException(nameof(orderApiService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        OrderDate = DateTime.Now;
        
        _autoSaveTimer = new DispatcherTimer();
        _autoSaveTimer.Interval = TimeSpan.FromMilliseconds(1000); // 1 second delay
        _autoSaveTimer.Tick += AutoSaveTimer_Tick;
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
                        AddOrderItem(new OrderItemViewModel
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
    
    public async void InitializeNewOrder()
    {
        IsLoading = true;
        try
        {
            // Get user's single draft order (Get or Create)
            var draftOrder = await _orderApiService.GetDraftOrderAsync();
            
            if (draftOrder != null)
            {
                Id = draftOrder.Id;
                OrderId = $"#ORD-{draftOrder.Id:D4}";
                OrderStatus = "DRAFT";
                SelectedStatus = "DRAFT";
                OrderDate = draftOrder.CreatedTime;
                
                // If draft has content (items or customer), prompt user
                if (draftOrder.CustomerId.HasValue || (draftOrder.OrderItems != null && draftOrder.OrderItems.Count > 0))
                {
                    var dialog = new ContentDialog
                    {
                        XamlRoot = App.Current.MainWindow.Content.XamlRoot,
                        Title = "Đơn hàng chưa hoàn thành",
                        Content = "Bạn có một đơn hàng đang soạn dở. Bạn có muốn tiếp tục không?",
                        PrimaryButtonText = "Tiếp tục đơn cũ",
                        SecondaryButtonText = "Tạo mới (Xóa cũ)",
                        DefaultButton = ContentDialogButton.Primary
                    };

                    var result = await dialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        // Load existing draft content
                        LoadDraftContent(draftOrder);
                    }
                    else
                    {
                        // Clear draft content (reset to empty) via AutoSave with empty data? 
                        // Or just clear UI and let next AutoSave overwrite it.
                        // Better to clear UI and set IsEditing = true immediately.
                        ClearForm();
                        // Trigger immediate autosave to clear backend
                        await PerformAutoSaveAsync();
                    }
                }
                else
                {
                    // Empty draft, just use it
                    ClearForm();
                }

                IsNewOrder = false; // Effectively it's an existing draft order in DB
                IsEditing = true;   // But we are editing it
            }
        }
        catch (Exception ex)
        {
            ShowNotification($"Lỗi tải đơn nháp: {ex.Message}", "Error");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void LoadDraftContent(ApiOrder order)
    {
        CustomerId = order.CustomerId;
        CustomerName = order.Customer?.Name ?? "";
        CustomerEmail = order.Customer?.Email ?? "";
        CustomerPhone = order.Customer?.Phone ?? "";
        CustomerAddress = order.Customer?.Address ?? "";
        CustomerAvatar = order.Customer?.AvatarUrl ?? "";

        OrderItems.Clear();
        if (order.OrderItems != null)
        {
            foreach (var item in order.OrderItems)
            {
                AddOrderItem(new OrderItemViewModel
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name ?? "Unknown",
                    ProductSku = item.Product?.Sku ?? "",
                    ProductImage = (item.Product?.Images?.Count > 0) ? item.Product?.Images[0].Url : "",
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitSalePrice,
                    TotalPrice = item.TotalPrice
                });
            }
        }
        RecalculateTotals();
    }

    private void ClearForm()
    {
        CustomerId = null;
        CustomerName = "";
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
        
        // Trigger auto-save whenever totals change (implies items/quantity changed)
        TriggerAutoSave();
    }
    
    // Auto-save Logic
    private void TriggerAutoSave()
    {
        // Only auto-save if editing
        if (!IsEditing) return;
        
        // Stop previous timer and restart
        _autoSaveTimer.Stop();
        _autoSaveTimer.Start();
    }
    
    private async void AutoSaveTimer_Tick(object sender, object e)
    {
        _autoSaveTimer.Stop();
        await PerformAutoSaveAsync();
    }
    
    private async Task PerformAutoSaveAsync()
    {
        // Must have an ID (which we should, from InitializeNewOrder)
        if (Id == 0) return;
        
        try
        {
            var request = new CreateOrderRequest
            {
                CustomerId = CustomerId,
                Status = "DRAFT",
                Items = OrderItems.Select(i => new CreateOrderItemRequest
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            };
            
            // Allow autosave even with empty items/customer to support clearing draft
            await _orderApiService.AutosaveOrderAsync(Id, request);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Auto-save failed: {ex.Message}");
        }
    }

    private void AddOrderItem(OrderItemViewModel item)
    {
        item.QuantityChanged += RecalculateTotals;
        OrderItems.Add(item);
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
            
            TriggerAutoSave();
        }
    }

    [RelayCommand]
    private async Task AddProduct()
    {
        var dialog = new ProductSelectionDialog
        {
            XamlRoot = App.Current.MainWindow.Content.XamlRoot
        };
        
        // Load products when dialog opens
        dialog.LoadProducts();
        
        var result = await dialog.ShowAsync();
        
        if (result == ContentDialogResult.Primary && dialog.ViewModel.SelectedProduct != null)
        {
            var product = dialog.ViewModel.SelectedProduct;
            var existing = OrderItems.FirstOrDefault(i => i.ProductId == product.Id);
            
            if (existing != null)
            {
                existing.Quantity++;
                existing.TotalPrice = existing.Quantity * existing.UnitPrice;
            }
            else
            {
                AddOrderItem(new OrderItemViewModel
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
        // Global Validation
        if (!CustomerId.HasValue)
        {
            ShowNotification("Vui lòng chọn khách hàng.", "Warning");
            return;
        }

        if (OrderItems.Count == 0)
        {
            ShowNotification("Vui lòng thêm sản phẩm.", "Warning");
            return;
        }

        IsLoading = true;
        try
        {
            // Flush any pending auto-save
            if (_autoSaveTimer.IsEnabled)
            {
                _autoSaveTimer.Stop();
                await PerformAutoSaveAsync();
            }

            // Finalize the order: Update status to PENDING
            // Since we always have an ID (draft), we just update status
            var result = await _orderApiService.UpdateStatusAsync(Id, "PENDING");
            
            if (result != null)
            {
                OrderStatus = result.Status;
                SelectedStatus = result.Status;
                
                System.Diagnostics.Debug.WriteLine($"Order finalized with status {OrderStatus}");
                IsEditing = false;
                IsNewOrder = false; // It's no longer a 'new' creation flow
                
                ShowNotification("Đơn hàng đã được tạo thành công!", "Success");
                
                // Perhaps navigate back or clear form for next order?
                // For now, stay on detail view as finalized order
            }
            else
            {
                 ShowNotification("Không thể lưu đơn hàng.", "Error");
            }
        }
        catch (Exception ex)
        {
            ShowNotification($"Lỗi: {ex.Message}", "Error");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowNotification(string message, string severity)
    {
        InfoBarMessage = message;
        InfoBarSeverity = severity;
        IsTipOpen = true;
    }

    [RelayCommand]
    private void Cancel()
    {
        if (IsNewOrder)
        {
            _navigationService.GoBack();
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
        var dialog = new ContentDialog
        {
            XamlRoot = App.Current.MainWindow.Content.XamlRoot,
            Title = "Xác nhận xóa",
            Content = "Bạn có chắc chắn muốn xóa đơn hàng này không? Hành động này không thể hoàn tác.",
            PrimaryButtonText = "Xóa",
            CloseButtonText = "Hủy",
            DefaultButton = ContentDialogButton.Close
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary) return;

        IsLoading = true;
        
        try
        {
            var success = await _orderApiService.DeleteOrderAsync(Id);
            if (success)
            {
                ShowNotification("Đơn hàng đã được xóa thành công!", "Success");
                
                // Wait briefly for user to see notification
                await Task.Delay(1000);
                
                // Navigate back
                if (_navigationService.CanGoBack)
                {
                    _navigationService.GoBack();
                }
                else
                {
                    // Fallback to Orders List
                    _navigationService.Navigate(typeof(OrdersView));
                }
            }
            else
            {
                ShowNotification("Không thể xóa đơn hàng. Vui lòng thử lại.", "Error");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting order: {ex.Message}");
            ShowNotification($"Đã xảy ra lỗi: {ex.Message}", "Error");
        }
        finally
        {
            IsLoading = false;
        }
    }
}

/// <summary>
/// ViewModel for Order Item display in Order Detail
/// </summary>
public partial class OrderItemViewModel : ObservableObject
{
    /// <summary>
    /// Event raised when quantity changes, used to notify parent to recalculate totals
    /// </summary>
    public event Action? QuantityChanged;

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
        QuantityChanged?.Invoke();
    }
}

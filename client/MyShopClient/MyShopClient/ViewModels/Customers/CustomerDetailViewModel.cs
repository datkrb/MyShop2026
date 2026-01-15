using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using MyShopClient.Models;
using MyShopClient.Services.Api;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI;

namespace MyShopClient.ViewModels;

/// <summary>
/// ViewModel for Customer Detail View
/// </summary>
public partial class CustomerDetailViewModel : ObservableObject
{
    private readonly CustomerApiService _customerApiService;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _id;

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
    public string FormattedCreatedDate => $"Customer since {CreatedAt:MMMM yyyy}";
    public int OrderCount => RecentOrders.Count;

    public ObservableCollection<CustomerOrderViewModel> RecentOrders { get; } = new();

    public CustomerDetailViewModel(CustomerApiService customerApiService)
    {
        _customerApiService = customerApiService ?? throw new ArgumentNullException(nameof(customerApiService));
    }

    /// <summary>
    /// Load basic customer info from list (for immediate display)
    /// then fetch full details from API
    /// </summary>
    public void LoadCustomer(CustomerViewModel customer)
    {
        Id = customer.Id;
        Name = customer.Name;
        Email = customer.Email;
        Phone = customer.Phone;
        Address = customer.Address;
        CreatedAt = customer.CreatedAt;

        OnPropertyChanged(nameof(AvatarUrl));
        OnPropertyChanged(nameof(FormattedCreatedDate));

        // Load full details including orders from API
        _ = LoadCustomerDetailsAsync(customer.Id);
    }

    /// <summary>
    /// Load full customer details including orders from API
    /// </summary>
    public async Task LoadCustomerDetailsAsync(int customerId)
    {
        IsLoading = true;

        try
        {
            var customer = await _customerApiService.GetCustomerAsync(customerId);

            if (customer != null)
            {
                Id = customer.Id;
                Name = customer.Name;
                Email = customer.Email;
                Phone = customer.Phone;
                Address = customer.Address;
                CreatedAt = customer.CreatedAt;

                RecentOrders.Clear();

                if (customer.Orders != null)
                {
                    foreach (var order in customer.Orders)
                    {
                        RecentOrders.Add(new CustomerOrderViewModel
                        {
                            OrderId = $"#ORD-{order.Id:D4}",
                            Date = order.CreatedTime,
                            Amount = order.FinalPrice,
                            Status = FormatStatus(order.Status)
                        });
                    }
                }

                OnPropertyChanged(nameof(AvatarUrl));
                OnPropertyChanged(nameof(FormattedCreatedDate));
                OnPropertyChanged(nameof(OrderCount));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading customer details: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static string FormatStatus(string status)
    {
        return status switch
        {
            "PAID" => "Paid",
            "DRAFT" => "Draft",
            "CANCELLED" => "Cancelled",
            "PENDING" => "Pending",
            _ => status
        };
    }

    public async Task<bool> DeleteCustomerAsync()
    {
        try
        {
            return await _customerApiService.DeleteCustomerAsync(Id);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting customer: {ex.Message}");
            return false;
        }
    }
}

/// <summary>
/// Order item for customer order history
/// </summary>
public class CustomerOrderViewModel
{
    public string OrderId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;

    public string FormattedDate => Date.ToString("MMM dd, yyyy");
    public string FormattedAmount => $"{Amount:N0}đ";

    public SolidColorBrush StatusBackground => Status switch
    {
        "Paid" => new SolidColorBrush(Color.FromArgb(255, 220, 252, 231)),
        "Draft" => new SolidColorBrush(Color.FromArgb(255, 224, 231, 255)),
        "Pending" => new SolidColorBrush(Color.FromArgb(255, 254, 249, 195)),
        "Cancelled" => new SolidColorBrush(Color.FromArgb(255, 254, 226, 226)),
        _ => new SolidColorBrush(Color.FromArgb(255, 241, 245, 249))
    };

    public SolidColorBrush StatusForeground => Status switch
    {
        "Paid" => new SolidColorBrush(Color.FromArgb(255, 22, 163, 74)),
        "Draft" => new SolidColorBrush(Color.FromArgb(255, 124, 92, 252)),
        "Pending" => new SolidColorBrush(Color.FromArgb(255, 202, 138, 4)),
        "Cancelled" => new SolidColorBrush(Color.FromArgb(255, 239, 68, 68)),
        _ => new SolidColorBrush(Color.FromArgb(255, 100, 116, 139))
    };
}

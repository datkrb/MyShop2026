using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using Windows.UI;

namespace MyShopClient.ViewModels;

/// <summary>
/// ViewModel for Customer Detail View
/// </summary>
public partial class CustomerDetailViewModel : ObservableObject
{
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

    public CustomerDetailViewModel()
    {
    }

    public void LoadCustomer(CustomerViewModel customer)
    {
        Id = customer.Id;
        Name = customer.Name;
        Email = customer.Email;
        Phone = customer.Phone;
        Address = customer.Address;
        CreatedAt = customer.CreatedAt;

        // Load mock order history
        RecentOrders.Clear();
        RecentOrders.Add(new CustomerOrderViewModel { OrderId = "#ORD-2023-001", Date = DateTime.Now.AddDays(-5), Amount = 250.00m, Status = "Paid" });
        RecentOrders.Add(new CustomerOrderViewModel { OrderId = "#ORD-2023-008", Date = DateTime.Now.AddDays(-15), Amount = 180.50m, Status = "Paid" });
        RecentOrders.Add(new CustomerOrderViewModel { OrderId = "#ORD-2023-015", Date = DateTime.Now.AddDays(-30), Amount = 420.00m, Status = "Canceled" });

        OnPropertyChanged(nameof(AvatarUrl));
        OnPropertyChanged(nameof(FormattedCreatedDate));
        OnPropertyChanged(nameof(OrderCount));
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
    public string FormattedAmount => $"${Amount:N2}";

    public SolidColorBrush StatusBackground => Status switch
    {
        "Paid" => new SolidColorBrush(Color.FromArgb(255, 220, 252, 231)),
        "New" => new SolidColorBrush(Color.FromArgb(255, 224, 231, 255)),
        "Canceled" => new SolidColorBrush(Color.FromArgb(255, 254, 226, 226)),
        _ => new SolidColorBrush(Color.FromArgb(255, 241, 245, 249))
    };

    public SolidColorBrush StatusForeground => Status switch
    {
        "Paid" => new SolidColorBrush(Color.FromArgb(255, 22, 163, 74)),
        "New" => new SolidColorBrush(Color.FromArgb(255, 124, 92, 252)),
        "Canceled" => new SolidColorBrush(Color.FromArgb(255, 239, 68, 68)),
        _ => new SolidColorBrush(Color.FromArgb(255, 100, 116, 139))
    };
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace MyShopClient.Models;

using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>
/// Full Product model matching API responses for dashboard/top-selling, dashboard/low-stock
/// This model matches the API response structure exactly
/// </summary>
public partial class ApiProduct : ObservableObject
{
    public int Id { get; set; }
    
    [ObservableProperty]
    private string _sku = string.Empty;
    
    [ObservableProperty]
    private string _name = string.Empty;
    
    [ObservableProperty]
    private decimal _importPrice;
    
    [ObservableProperty]
    private decimal _salePrice;
    
    [ObservableProperty]
    private int _stock;
    
    [ObservableProperty]
    private string? _description;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    [ObservableProperty]
    private int? _categoryId;
    
    // Navigation property
    public Category? Category { get; set; }
    
    // For top-selling products (contains order items)
    public List<OrderItem>? OrderItems { get; set; }

    public List<ProductImage>? Images { get; set; } = new();
    
    // Computed property: total sold from orderItems
    public int TotalSold => OrderItems?.Sum(oi => oi.Quantity) ?? 0;

    public string FormattedPrice => Helpers.CurrencyHelper.FormatVND(SalePrice);
}

public class ProductImage
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public int ProductId { get; set; }
}

/// <summary>
/// Simple Product model for UI display (e.g., dashboard cards)
/// Used in DashboardViewModel for TopSelling and LowStock display
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = "https://via.placeholder.com/150";
    public int Sold { get; set; }
    public int Stock { get; set; }
}

public class ProductStats
{
    public int TotalProducts { get; set; }
    public int TotalCategories { get; set; }
    public int LowStock { get; set; }
    public int OutOfStock { get; set; }
}

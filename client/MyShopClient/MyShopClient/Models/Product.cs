using System;
using System.Collections.Generic;
using System.Linq;

namespace MyShopClient.Models;

/// <summary>
/// Full Product model matching API responses for dashboard/top-selling, dashboard/low-stock
/// This model matches the API response structure exactly
/// </summary>
public class ApiProduct
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal ImportPrice { get; set; }
    public decimal SalePrice { get; set; }
    public int Stock { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? CategoryId { get; set; }
    
    // Navigation property
    public Category? Category { get; set; }
    
    // For top-selling products (contains order items)
    public List<OrderItem>? OrderItems { get; set; }
    
    // Computed property: total sold from orderItems
    public int TotalSold => OrderItems?.Sum(oi => oi.Quantity) ?? 0;
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

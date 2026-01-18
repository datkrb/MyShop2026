using System;
using System.Collections.Generic;
using System.Linq;

namespace MyShopClient.Models;

/// <summary>
/// User info from API (createdBy field)
/// </summary>
public class ApiUser
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// Full Order model matching API: /api/orders
/// </summary>
public class ApiOrder
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedTime { get; set; }
    public DateTime UpdatedAt { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public int? PromotionId { get; set; }
    public int? CustomerId { get; set; }
    public int? CreatedById { get; set; }
    
    // Navigation properties
    public Customer? Customer { get; set; }
    public ApiUser? CreatedBy { get; set; }
    public Promotion? Promotion { get; set; }
    public List<OrderItem>? OrderItems { get; set; }
    
    // Helper: Get first item name for display
    public string FirstItemName => OrderItems?.FirstOrDefault()?.Product?.Name ?? "Order Items";
    
    // Helper: Get total items count
    public int TotalItems => OrderItems?.Sum(oi => oi.Quantity) ?? 0;
}

/// <summary>
/// Simple Order model for UI display (dashboard recent orders table)
/// </summary>
public class Order
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerAvatar { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Price { get; set; }
    
    /// <summary>
    /// Người thực hiện đơn hàng (sale/admin username)
    /// </summary>
    public string CreatedByName { get; set; } = string.Empty;
    
    /// <summary>
    /// Role của người thực hiện (ADMIN/SALE)
    /// </summary>
    public string CreatedByRole { get; set; } = string.Empty;
    
    /// <summary>
    /// Create Order from ApiOrder for UI display
    /// </summary>
    public static Order FromApiOrder(ApiOrder apiOrder)
    {
        return new Order
        {
            OrderId = $"#{apiOrder.Id:D6}",
            CustomerName = apiOrder.Customer?.Name ?? "Walk-in Customer",
            CustomerAvatar = apiOrder.Customer?.AvatarUrl ?? $"https://ui-avatars.com/api/?name=Guest&background=7C5CFC&color=fff",
            ItemName = apiOrder.FirstItemName,
            OrderDate = apiOrder.CreatedTime,
            Status = apiOrder.Status,
            Price = apiOrder.FinalPrice,
            CreatedByName = apiOrder.CreatedBy?.Username ?? "Unknown",
            CreatedByRole = apiOrder.CreatedBy?.Role ?? ""
        };
    }
}


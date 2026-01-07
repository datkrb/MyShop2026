using System;

namespace MyShopClient.Models;

/// <summary>
/// OrderItem model matching API: dashboard/recent-orders -> orderItems
/// </summary>
public class OrderItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitSalePrice { get; set; }
    public decimal TotalPrice { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    
    // Nested product reference
    public ApiProduct? Product { get; set; }
}

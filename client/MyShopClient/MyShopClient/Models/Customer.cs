using System;
using System.Collections.Generic;

namespace MyShopClient.Models;

/// <summary>
/// Customer model matching API: /api/customers
/// </summary>
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Orders from detail API
    public List<CustomerOrder>? Orders { get; set; }
    
    // Helper property for avatar (not from API)
    public string AvatarUrl => $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(Name)}&background=7C5CFC&color=fff";
}

/// <summary>
/// Customer's order from detail API response
/// </summary>
public class CustomerOrder
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedTime { get; set; }
    public DateTime UpdatedAt { get; set; }
    public decimal FinalPrice { get; set; }
    public int CustomerId { get; set; }
    public int CreatedById { get; set; }
}

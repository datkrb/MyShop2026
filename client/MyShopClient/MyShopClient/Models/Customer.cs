using System;

namespace MyShopClient.Models;

/// <summary>
/// Customer model matching API: dashboard/recent-orders
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
    
    // Helper property for avatar (not from API)
    public string AvatarUrl => $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(Name)}&background=7C5CFC&color=fff";
}

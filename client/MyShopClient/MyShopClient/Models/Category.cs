using System;

namespace MyShopClient.Models;

/// <summary>
/// Category model matching API: dashboard/top-selling, dashboard/low-stock
/// </summary>
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

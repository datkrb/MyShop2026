using System;
using System.Collections.Generic;

namespace MyShopClient.Models;

/// <summary>
/// Dashboard Summary model matching API: dashboard/summary
/// </summary>
public class DashboardSummary
{
    public int TotalProducts { get; set; }
    public int TotalOrdersToday { get; set; }
    public decimal RevenueToday { get; set; }
    public int PendingOrders { get; set; }
    // Percentage changes since yesterday
    public decimal ProductsChange { get; set; }
    public decimal OrdersChange { get; set; }
    public decimal RevenueChange { get; set; }
}

/// <summary>
/// Revenue Chart Data for dashboard/revenue-chart
/// </summary>
public class RevenueChartData
{
    public Dictionary<int, decimal> DailyRevenue { get; set; } = new();
}

/// <summary>
/// Category stats for pie chart: dashboard/category-stats
/// </summary>
public class CategoryStat
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ProductCount { get; set; }
}

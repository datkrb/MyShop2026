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
}

/// <summary>
/// Revenue Chart Data for dashboard/revenue-chart
/// </summary>
public class RevenueChartData
{
    public Dictionary<int, decimal> DailyRevenue { get; set; } = new();
}

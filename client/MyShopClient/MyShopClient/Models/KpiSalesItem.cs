using System;

namespace MyShopClient.Models;

/// <summary>
/// KPI Sales report item - represents sales performance for a user
/// </summary>
public class KpiSalesItem
{
    public KpiUserInfo User { get; set; } = new();
    public int Orders { get; set; }
    public decimal Revenue { get; set; }
    public decimal Commission { get; set; }
    public int CommissionRate { get; set; } // 3, 5, or 7 percent

    // Formatted display properties with units
    public string RevenueDisplay => Helpers.CurrencyHelper.FormatVND(Revenue);
    public string CommissionDisplay => Helpers.CurrencyHelper.FormatVND(Commission);
    public string CommissionRateDisplay => $"{CommissionRate}%";
}

/// <summary>
/// User info in KPI report
/// </summary>
public class KpiUserInfo
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

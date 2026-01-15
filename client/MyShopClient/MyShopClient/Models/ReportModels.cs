using System;

namespace MyShopClient.Models;

public class RevenueReportItem
{
    public string Date { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
}

public class ProfitReport
{
    public decimal Revenue { get; set; }
    public decimal Cost { get; set; }
    public decimal Profit { get; set; }
    public decimal ProfitMargin { get; set; }
}

public class ProductSalesItem
{
    public ApiProduct Product { get; set; } = new();
    public int Quantity { get; set; }
    public decimal Revenue { get; set; }
}

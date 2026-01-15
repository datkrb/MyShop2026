using System;
using System.Collections.Generic;

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

public class TopProductsTimeSeriesReport
{
    public List<ProductInfo> Products { get; set; } = new();
    public List<string> Dates { get; set; } = new();
    public List<ProductSeriesData> Series { get; set; } = new();
}

public class ProductInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ProductSeriesData
{
    public int ProductId { get; set; }
    public List<int> Data { get; set; } = new();
}

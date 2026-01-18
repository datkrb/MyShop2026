using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyShopClient.Models;

namespace MyShopClient.Services.Api;

public class ReportApiService : BaseApiService, IReportApiService
{
    public ReportApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<List<RevenueReportItem>> GetRevenueReportAsync(DateTime startDate, DateTime endDate, string type = "day", int? categoryId = null)
    {
        var query = $"reports/revenue?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&type={type}";
        if (categoryId.HasValue)
        {
            query += $"&categoryId={categoryId.Value}";
        }
        return await GetAsync<List<RevenueReportItem>>(query) ?? new List<RevenueReportItem>();
    }

    public async Task<ProfitReport> GetProfitReportAsync(DateTime startDate, DateTime endDate, int? categoryId = null)
    {
        var query = $"reports/profit?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
        if (categoryId.HasValue)
        {
            query += $"&categoryId={categoryId.Value}";
        }
        return await GetAsync<ProfitReport>(query) ?? new ProfitReport();
    }

    public async Task<List<ProductSalesItem>> GetProductSalesReportAsync(DateTime startDate, DateTime endDate)
    {
        var query = $"reports/products?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
        return await GetAsync<List<ProductSalesItem>>(query) ?? new List<ProductSalesItem>();
    }

    public async Task<TopProductsTimeSeriesReport> GetTopProductsTimeSeriesAsync(DateTime startDate, DateTime endDate, int? categoryId = null)
    {
        var query = $"reports/products/timeseries?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
        if (categoryId.HasValue)
        {
            query += $"&categoryId={categoryId.Value}";
        }
        return await GetAsync<TopProductsTimeSeriesReport>(query) ?? new TopProductsTimeSeriesReport();
    }

    /// <summary>
    /// Get KPI sales report for all users (ADMIN only)
    /// </summary>
    public async Task<List<KpiSalesItem>> GetKpiSalesReportAsync(int year, int? month = null)
    {
        var query = $"reports/kpi-sales?year={year}";
        if (month.HasValue)
        {
            query += $"&month={month.Value}";
        }
        return await GetAsync<List<KpiSalesItem>>(query) ?? new List<KpiSalesItem>();
    }

    /// <summary>
    /// Get own KPI report (for SALE users)
    /// </summary>
    public async Task<KpiSalesItem?> GetMyKpiAsync(int year, int? month = null)
    {
        var query = $"reports/my-kpi?year={year}";
        if (month.HasValue)
        {
            query += $"&month={month.Value}";
        }
        var result = await GetAsync<List<KpiSalesItem>>(query);
        return result?.Count > 0 ? result[0] : null;
    }

    /// <summary>
    /// Get profit time series data
    /// </summary>
    public async Task<List<ProfitTimeSeriesItem>> GetProfitTimeSeriesAsync(DateTime startDate, DateTime endDate, string type = "day", int? categoryId = null)
    {
        var query = $"reports/profit-series?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&type={type}";
        if (categoryId.HasValue)
        {
            query += $"&categoryId={categoryId.Value}";
        }
        return await GetAsync<List<ProfitTimeSeriesItem>>(query) ?? new List<ProfitTimeSeriesItem>();
    }

    /// <summary>
    /// Get sales time series for a specific product
    /// </summary>
    public async Task<List<ProductSalesTimeSeriesItem>> GetProductSalesByIdAsync(int productId, DateTime startDate, DateTime endDate, string type = "day")
    {
        var query = $"reports/products/{productId}/sales?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&type={type}";
        return await GetAsync<List<ProductSalesTimeSeriesItem>>(query) ?? new List<ProductSalesTimeSeriesItem>();
    }

    /// <summary>
    /// Get sales time series grouped by category
    /// </summary>
    public async Task<CategorySalesTimeSeriesReport> GetCategorySalesTimeSeriesAsync(DateTime startDate, DateTime endDate, string type = "day")
    {
        var query = $"reports/category-sales?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&type={type}";
        return await GetAsync<CategorySalesTimeSeriesReport>(query) ?? new CategorySalesTimeSeriesReport();
    }
}

public interface IReportApiService
{
    Task<List<RevenueReportItem>> GetRevenueReportAsync(DateTime startDate, DateTime endDate, string type = "day", int? categoryId = null);
    Task<ProfitReport> GetProfitReportAsync(DateTime startDate, DateTime endDate, int? categoryId = null);
    Task<List<ProductSalesItem>> GetProductSalesReportAsync(DateTime startDate, DateTime endDate);
    Task<TopProductsTimeSeriesReport> GetTopProductsTimeSeriesAsync(DateTime startDate, DateTime endDate, int? categoryId = null);
    Task<List<KpiSalesItem>> GetKpiSalesReportAsync(int year, int? month = null);
    Task<KpiSalesItem?> GetMyKpiAsync(int year, int? month = null);
    Task<List<ProfitTimeSeriesItem>> GetProfitTimeSeriesAsync(DateTime startDate, DateTime endDate, string type = "day", int? categoryId = null);
    Task<List<ProductSalesTimeSeriesItem>> GetProductSalesByIdAsync(int productId, DateTime startDate, DateTime endDate, string type = "day");
    Task<CategorySalesTimeSeriesReport> GetCategorySalesTimeSeriesAsync(DateTime startDate, DateTime endDate, string type = "day");
}

// New Models for Report Charts
public class ProfitTimeSeriesItem
{
    public string Date { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal Cost { get; set; }
    public decimal Profit { get; set; }
}

public class ProductSalesTimeSeriesItem
{
    public string Date { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Revenue { get; set; }
}

public class CategorySalesTimeSeriesReport
{
    public List<string> Dates { get; set; } = new();
    public List<CategorySalesSeriesItem> Series { get; set; } = new();
}

public class CategorySalesSeriesItem
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<int> Data { get; set; } = new();
}



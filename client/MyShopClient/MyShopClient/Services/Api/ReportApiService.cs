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
}

public interface IReportApiService
{
    Task<List<RevenueReportItem>> GetRevenueReportAsync(DateTime startDate, DateTime endDate, string type = "day", int? categoryId = null);
    Task<ProfitReport> GetProfitReportAsync(DateTime startDate, DateTime endDate, int? categoryId = null);
    Task<List<ProductSalesItem>> GetProductSalesReportAsync(DateTime startDate, DateTime endDate);
    Task<TopProductsTimeSeriesReport> GetTopProductsTimeSeriesAsync(DateTime startDate, DateTime endDate, int? categoryId = null);
}


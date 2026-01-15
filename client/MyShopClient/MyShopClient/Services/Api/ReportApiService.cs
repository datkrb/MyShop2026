using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyShopClient.Models;

namespace MyShopClient.Services.Api;

public class ReportApiService : BaseApiService, IReportApiService
{
    private static ReportApiService? _instance;
    public static ReportApiService Instance => _instance ??= new ReportApiService();

    private ReportApiService() : base() { }

    public async Task<List<RevenueReportItem>> GetRevenueReportAsync(DateTime startDate, DateTime endDate, string type = "day")
    {
        var query = $"reports/revenue?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&type={type}";
        return await GetAsync<List<RevenueReportItem>>(query) ?? new List<RevenueReportItem>();
    }

    public async Task<ProfitReport> GetProfitReportAsync(DateTime startDate, DateTime endDate)
    {
        var query = $"reports/profit?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
        return await GetAsync<ProfitReport>(query) ?? new ProfitReport();
    }

    public async Task<List<ProductSalesItem>> GetProductSalesReportAsync(DateTime startDate, DateTime endDate)
    {
        var query = $"reports/products?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
        return await GetAsync<List<ProductSalesItem>>(query) ?? new List<ProductSalesItem>();
    }
}

public interface IReportApiService
{
    Task<List<RevenueReportItem>> GetRevenueReportAsync(DateTime startDate, DateTime endDate, string type = "day");
    Task<ProfitReport> GetProfitReportAsync(DateTime startDate, DateTime endDate);
    Task<List<ProductSalesItem>> GetProductSalesReportAsync(DateTime startDate, DateTime endDate);
}

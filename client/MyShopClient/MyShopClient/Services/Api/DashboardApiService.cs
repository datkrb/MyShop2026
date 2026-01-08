using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyShopClient.Models;

namespace MyShopClient.Services.Api;

public class DashboardApiService : BaseApiService
{
    private static DashboardApiService? _instance;
    public static DashboardApiService Instance => _instance ??= new DashboardApiService();

    /// <summary>
    /// Get dashboard summary (total products, orders today, revenue today)
    /// </summary>
    public async Task<DashboardSummary?> GetSummaryAsync()
    {
        return await GetAsync<DashboardSummary>("dashboard/summary");
    }

    /// <summary>
    /// Get low stock products
    /// </summary>
    public async Task<List<ApiProduct>?> GetLowStockAsync()
    {
        return await GetAsync<List<ApiProduct>>("dashboard/low-stock");
    }

    /// <summary>
    /// Get top selling products with calculated sold count
    /// </summary>
    public async Task<List<ApiProduct>?> GetTopSellingAsync()
    {
        return await GetAsync<List<ApiProduct>>("dashboard/top-selling");
    }

    /// <summary>
    /// Get revenue chart data (daily revenue by day of month)
    /// </summary>
    public async Task<Dictionary<string, decimal>?> GetRevenueChartAsync()
    {
        return await GetAsync<Dictionary<string, decimal>>("dashboard/revenue-chart");
    }

    /// <summary>
    /// Get recent orders with customer and order items
    /// </summary>
    public async Task<List<ApiOrder>?> GetRecentOrdersAsync()
    {
        return await GetAsync<List<ApiOrder>>("dashboard/recent-orders");
    }

    /// <summary>
    /// Get category stats (category name and product count)
    /// </summary>
    public async Task<List<CategoryStat>?> GetCategoryStatsAsync()
    {
        return await GetAsync<List<CategoryStat>>("dashboard/category-stats");
    }
}

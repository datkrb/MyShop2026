using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MyShopClient.Models;

namespace MyShopClient.Services.Api;

public class OrderApiService : BaseApiService
{
    private static OrderApiService? _instance;
    public static OrderApiService Instance => _instance ??= new OrderApiService();

    public OrderApiService() : base() { }

    public async Task<ApiResponse<List<ApiOrder>>> GetOrdersAsync(int page = 1, int pageSize = 10, string? keyword = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var queryParams = new List<string>
        {
            $"page={page}",
            $"limit={pageSize}"
        };

        if (!string.IsNullOrEmpty(keyword))
        {
            queryParams.Add($"keyword={Uri.EscapeDataString(keyword)}");
        }
        
        if (!string.IsNullOrEmpty(status) && status != "All")
        {
            queryParams.Add($"status={Uri.EscapeDataString(status)}");
        }

        if (fromDate.HasValue)
        {
             queryParams.Add($"startDate={fromDate.Value:yyyy-MM-dd}");
        }
        
        if (toDate.HasValue)
        {
             queryParams.Add($"endDate={toDate.Value:yyyy-MM-dd}");
        }

        string queryString = string.Join("&", queryParams);
        return await GetAsync<ApiResponse<List<ApiOrder>>>($"orders?{queryString}");
    }

    public async Task<ApiOrder?> GetOrderAsync(int id)
    {
        return await GetAsync<ApiOrder>($"orders/{id}");
    }

    public async Task<ApiOrder?> CreateOrderAsync(ApiOrder order)
    {
        return await PostAsync<ApiOrder>("orders", order);
    }

    public async Task<ApiOrder?> UpdateOrderAsync(int id, ApiOrder order)
    {
        return await PutAsync<ApiOrder>($"orders/{id}", order);
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        return await DeleteAsync($"orders/{id}");
    }
}

using System;
using System.Text;
using System.Threading.Tasks;
using MyShopClient.Models;

namespace MyShopClient.Services.Api;

public class OrderApiService : BaseApiService
{
    private static OrderApiService? _instance;
    public static OrderApiService Instance => _instance ??= new OrderApiService();

    public OrderApiService() : base() { }

    public async Task<PagedResult<ApiOrder>?> GetOrdersAsync(
        int page = 1, 
        int size = 10, 
        string? status = null, 
        DateTime? fromDate = null, 
        DateTime? toDate = null)
    {
        var query = new StringBuilder($"orders?page={page}&size={size}");

        if (!string.IsNullOrEmpty(status) && status != "All")
        {
            query.Append($"&status={Uri.EscapeDataString(status)}");
        }

        if (fromDate.HasValue)
        {
            query.Append($"&fromDate={fromDate.Value:yyyy-MM-dd}");
        }

        if (toDate.HasValue)
        {
            query.Append($"&toDate={toDate.Value:yyyy-MM-dd}");
        }

        return await GetAsync<PagedResult<ApiOrder>>(query.ToString());
    }

    public async Task<ApiOrder?> GetOrderAsync(int id)
    {
        return await GetAsync<ApiOrder>($"orders/{id}");
    }

    public async Task<ApiOrder?> CreateOrderAsync(CreateOrderRequest request)
    {
        return await PostAsync<ApiOrder>("orders", request);
    }

    public async Task<ApiOrder?> UpdateOrderAsync(int id, UpdateOrderRequest request)
    {
        return await PutAsync<ApiOrder>($"orders/{id}", request);
    }

    public async Task<ApiOrder?> UpdateStatusAsync(int id, string status)
    {
        return await PutAsync<ApiOrder>($"orders/{id}/status", new { status });
    }

    public async Task<ApiOrder?> AutosaveOrderAsync(int id, CreateOrderRequest request)
    {
        return await PostAsync<ApiOrder>($"orders/{id}/autosave", request);
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        return await DeleteAsync($"orders/{id}");
    }
}

/// <summary>
/// Request to create a new order
/// </summary>
public class CreateOrderRequest
{
    public int? CustomerId { get; set; }
    public string Status { get; set; } = "DRAFT";
    public System.Collections.Generic.List<CreateOrderItemRequest>? Items { get; set; }
}

/// <summary>
/// Request to update an order
/// </summary>
public class UpdateOrderRequest
{
    public int? CustomerId { get; set; }
    public string? Status { get; set; }
    public System.Collections.Generic.List<CreateOrderItemRequest>? Items { get; set; }
}

/// <summary>
/// Order item in create/update request
/// </summary>
public class CreateOrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

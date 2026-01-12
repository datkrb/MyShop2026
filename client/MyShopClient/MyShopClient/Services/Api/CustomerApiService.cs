using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MyShopClient.Models;
using System.Web;

namespace MyShopClient.Services.Api;

public class CustomerApiService : BaseApiService
{
    private static CustomerApiService? _instance;
    public static CustomerApiService Instance => _instance ??= new CustomerApiService();

    public async Task<PagedResult<Customer>?> GetCustomersAsync(
        int page = 1,
        int size = 10,
        string? keyword = null)
    {
        var query = new StringBuilder($"customers?page={page}&size={size}");

        if (!string.IsNullOrEmpty(keyword))
        {
            query.Append($"&keyword={System.Net.WebUtility.UrlEncode(keyword)}");
        }

        return await GetAsync<PagedResult<Customer>>(query.ToString());
    }

    public async Task<Customer?> GetCustomerAsync(int id)
    {
        return await GetAsync<Customer>($"customers/{id}");
    }

    public async Task<Customer?> CreateCustomerAsync(Customer customer)
    {
        return await PostAsync<Customer>("customers", new
        {
            name = customer.Name,
            email = customer.Email,
            phone = customer.Phone,
            address = customer.Address
        });
    }

    public async Task<Customer?> UpdateCustomerAsync(int id, Customer customer)
    {
        return await PutAsync<Customer>($"customers/{id}", new
        {
            name = customer.Name,
            email = customer.Email,
            phone = customer.Phone,
            address = customer.Address
        });
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        return await DeleteAsync($"customers/{id}");
    }
}

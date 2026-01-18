using MyShopClient.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MyShopClient.Services.Api;

public class EmployeeApiService : BaseApiService
{
    public EmployeeApiService(HttpClient httpClient) : base(httpClient)
    {
    }
    public async Task<List<User>> GetAllAsync()
    {
        var result = await GetAsync<List<User>>("/employees");
        return result ?? new List<User>();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await GetAsync<User>($"/employees/{id}");
    }

    public async Task<User?> CreateAsync(CreateEmployeeRequest request)
    {
        return await PostAsync<User>("/employees", request);
    }

    public async Task<User?> UpdateAsync(int id, UpdateEmployeeRequest request)
    {
        return await PutAsync<User>($"/employees/{id}", request);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            await DeleteAsync($"/employees/{id}");
            return true;
        }
        catch
        {
            return false;
        }
    }
}

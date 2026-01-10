using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using MyShopClient.Models;
using System.Web;

namespace MyShopClient.Services.Api;

public class ProductApiService : BaseApiService
{
    private static ProductApiService? _instance;
    public static ProductApiService Instance => _instance ??= new ProductApiService();

    public async Task<PagedResult<ApiProduct>?> GetProductsAsync(
        int page = 1, 
        int size = 10, 
        int? categoryId = null, 
        string? keyword = null, 
        string sort = "id,desc",
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? id = null)
    {
        var query = new StringBuilder($"products?page={page}&size={size}");
        
        if (categoryId.HasValue)
        {
            query.Append($"&categoryId={categoryId.Value}");
        }
        
        if (!string.IsNullOrEmpty(keyword))
        {
            query.Append($"&keyword={System.Net.WebUtility.UrlEncode(keyword)}");
        }
        
        if (!string.IsNullOrEmpty(sort))
        {
            query.Append($"&sort={sort}");
        }

        if (minPrice.HasValue)
        {
            query.Append($"&minPrice={minPrice.Value}");
        }

        if (maxPrice.HasValue)
        {
            query.Append($"&maxPrice={maxPrice.Value}");
        }

        if (id.HasValue)
        {
            query.Append($"&id={id.Value}");
        }

        return await GetAsync<PagedResult<ApiProduct>>(query.ToString());
    }

    public async Task<List<Category>?> GetCategoriesAsync()
    {
        return await GetAsync<List<Category>>("categories");
    }

    public async Task<ApiProduct?> GetProductAsync(int id)
    {
        return await GetAsync<ApiProduct>($"products/{id}");
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        return await DeleteAsync($"products/{id}");
    }

    public async Task<ApiProduct?> UpdateProductAsync(int id, ApiProduct product)
    {
        // For update, we might need a specific DTO, but usually API accepts partial or full object
        // Based on logic, we usually send the object. 
        // Note: The API Controller expects req.body matching the structure. 
        return await PutAsync<ApiProduct>($"products/{id}", new 
        {
            name = product.Name,
            sku = product.Sku,
            importPrice = product.ImportPrice,
            salePrice = product.SalePrice,
            stock = product.Stock,
            categoryId = product.CategoryId,
            description = product.Description
        });
    }

    public async Task<ApiProduct?> CreateProductAsync(ApiProduct product)
    {
        return await PostAsync<ApiProduct>("products", new
        {
            name = product.Name,
            sku = product.Sku,
            importPrice = product.ImportPrice,
            salePrice = product.SalePrice,
            stock = product.Stock,
            categoryId = product.CategoryId,
            description = product.Description
        });
    }

    public async Task<Category?> CreateCategoryAsync(string name, string? description)
    {
        return await PostAsync<Category>("categories", new
        {
            name = name,
            description = description
        });
    }
}

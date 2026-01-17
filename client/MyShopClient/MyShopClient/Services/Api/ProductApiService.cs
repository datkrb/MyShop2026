using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using MyShopClient.Models;
using System.Web;

namespace MyShopClient.Services.Api;

public class ProductApiService : BaseApiService
{
    public ProductApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<PagedResult<ApiProduct>?> GetProductsAsync(
        int page = 1, 
        int size = 10, 
        int? categoryId = null, 
        string? keyword = null, 
        string sort = "id,desc",
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? id = null,
        // Advanced search parameters
        string? stockStatus = null,
        string? createdFrom = null,
        string? createdTo = null,
        int[]? categoryIds = null,
        string? skuSearch = null,
        string? skuMode = null,
        bool? inStock = null)
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

        // Advanced search parameters
        if (!string.IsNullOrEmpty(stockStatus) && stockStatus != "all")
        {
            query.Append($"&stockStatus={stockStatus}");
        }

        if (!string.IsNullOrEmpty(createdFrom))
        {
            query.Append($"&createdFrom={createdFrom}");
        }

        if (!string.IsNullOrEmpty(createdTo))
        {
            query.Append($"&createdTo={createdTo}");
        }

        if (categoryIds != null && categoryIds.Length > 0)
        {
            query.Append($"&categoryIds={string.Join(",", categoryIds)}");
        }

        if (!string.IsNullOrEmpty(skuSearch))
        {
            query.Append($"&skuSearch={System.Net.WebUtility.UrlEncode(skuSearch)}");
        }

        if (!string.IsNullOrEmpty(skuMode))
        {
            query.Append($"&skuMode={skuMode}");
        }

        if (inStock.HasValue)
        {
            query.Append($"&inStock={inStock.Value.ToString().ToLower()}");
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

    public async Task<ProductStats?> GetProductStatsAsync()
    {
        return await GetAsync<ProductStats>("products/stats");
    }

    public async Task<bool> UploadProductImagesAsync(int productId, List<string> imagePaths)
    {
        try
        {
            using var content = new System.Net.Http.MultipartFormDataContent();
            
            foreach (var path in imagePaths)
            {
                try
                {
                    if (!System.IO.File.Exists(path))
                    {
                        System.Diagnostics.Debug.WriteLine($"UploadProductImagesAsync: File not found: {path}");
                        continue;
                    }
                    
                    var fileName = System.IO.Path.GetFileName(path);
                    
                    // Read file into memory to avoid file locking
                    byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(path);
                    System.Diagnostics.Debug.WriteLine($"UploadProductImagesAsync: Read {fileBytes.Length} bytes from {fileName}");
                    
                    var byteContent = new System.Net.Http.ByteArrayContent(fileBytes);
                    
                    // Set content type based on file extension
                    var extension = System.IO.Path.GetExtension(path).ToLower();
                    var contentType = extension switch
                    {
                        ".png" => "image/png",
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".gif" => "image/gif",
                        ".webp" => "image/webp",
                        _ => "image/jpeg"
                    };
                    
                    byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                    content.Add(byteContent, "images", fileName);
                    System.Diagnostics.Debug.WriteLine($"UploadProductImagesAsync: Added file {fileName} to multipart content");
                }
                catch (Exception fileEx)
                {
                    System.Diagnostics.Debug.WriteLine($"UploadProductImagesAsync: Error reading file {path}: {fileEx.Message}");
                }
            }

            // Apply current token before request
            ApplyCurrentToken();
            
            // Build full URL (same pattern as base class methods)
            var fullUrl = BaseApiService._currentBaseUrl + $"products/{productId}/images";
            System.Diagnostics.Debug.WriteLine($"UploadProductImagesAsync: Uploading to {fullUrl}");
            
            var response = await _httpClient.PostAsync(fullUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"UploadProductImagesAsync: Response {response.StatusCode}: {responseContent}");
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UploadProductImagesAsync: Error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteProductImageAsync(int imageId)
    {
        return await DeleteAsync($"products/images/{imageId}");
    }
}

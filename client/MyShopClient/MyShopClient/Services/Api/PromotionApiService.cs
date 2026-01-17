using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MyShopClient.Models;

namespace MyShopClient.Services.Api;

public class PromotionApiService : BaseApiService
{
    public PromotionApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<PagedResult<Promotion>?> GetPromotionsAsync(
        int page = 1,
        int size = 10,
        bool? isActive = null,
        string? search = null,
        // Advanced search parameters
        string? discountType = null,
        string? validFrom = null,
        string? validTo = null,
        decimal? minDiscount = null,
        decimal? maxDiscount = null)
    {
        var query = new StringBuilder($"promotions?page={page}&size={size}");

        if (isActive.HasValue)
        {
            query.Append($"&isActive={isActive.Value}");
        }

        if (!string.IsNullOrEmpty(search))
        {
            query.Append($"&search={System.Net.WebUtility.UrlEncode(search)}");
        }

        // Advanced search parameters
        if (!string.IsNullOrEmpty(discountType))
        {
            query.Append($"&discountType={discountType}");
        }

        if (!string.IsNullOrEmpty(validFrom))
        {
            query.Append($"&validFrom={validFrom}");
        }

        if (!string.IsNullOrEmpty(validTo))
        {
            query.Append($"&validTo={validTo}");
        }

        if (minDiscount.HasValue)
        {
            query.Append($"&minDiscount={minDiscount.Value}");
        }

        if (maxDiscount.HasValue)
        {
            query.Append($"&maxDiscount={maxDiscount.Value}");
        }

        return await GetAsync<PagedResult<Promotion>>(query.ToString());
    }

    public async Task<Promotion?> GetPromotionAsync(int id)
    {
        return await GetAsync<Promotion>($"promotions/{id}");
    }

    public async Task<Promotion?> CreatePromotionAsync(Promotion promotion)
    {
        return await PostAsync<Promotion>("promotions", new
        {
            code = promotion.Code,
            description = promotion.Description,
            discountType = promotion.DiscountType,
            discountValue = promotion.DiscountValue,
            minOrderValue = promotion.MinOrderValue,
            maxDiscount = promotion.MaxDiscount,
            usageLimit = promotion.UsageLimit,
            startDate = promotion.StartDate,
            endDate = promotion.EndDate,
            isActive = promotion.IsActive
        });
    }

    public async Task<Promotion?> UpdatePromotionAsync(int id, Promotion promotion)
    {
        return await PutAsync<Promotion>($"promotions/{id}", new
        {
            code = promotion.Code,
            description = promotion.Description,
            discountType = promotion.DiscountType,
            discountValue = promotion.DiscountValue,
            minOrderValue = promotion.MinOrderValue,
            maxDiscount = promotion.MaxDiscount,
            usageLimit = promotion.UsageLimit,
            startDate = promotion.StartDate,
            endDate = promotion.EndDate,
            isActive = promotion.IsActive
        });
    }

    public async Task<bool> DeletePromotionAsync(int id)
    {
        return await DeleteAsync($"promotions/{id}");
    }

    public async Task<PromotionValidationResult?> ValidatePromotionAsync(string code, decimal orderValue)
    {
        return await PostAsync<PromotionValidationResult>("promotions/validate", new
        {
            code = code,
            orderValue = orderValue
        });
    }
}

public class PromotionValidationResult
{
    public bool Valid { get; set; }
    public Promotion? Promotion { get; set; }
    public decimal? DiscountAmount { get; set; }
    public string? Message { get; set; }
}

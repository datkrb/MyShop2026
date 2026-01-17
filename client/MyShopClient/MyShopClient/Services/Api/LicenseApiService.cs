using System.Net.Http;
using System.Threading.Tasks;
using MyShopClient.Models;

namespace MyShopClient.Services.Api;

/// <summary>
/// Service để gọi API license
/// </summary>
public class LicenseApiService : BaseApiService
{
    public LicenseApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    /// <summary>
    /// Lấy trạng thái license hiện tại
    /// </summary>
    public async Task<LicenseStatus?> GetStatusAsync()
    {
        return await GetAsync<LicenseStatus>("license/status");
    }

    /// <summary>
    /// Kích hoạt license với key
    /// </summary>
    public async Task<ActivationResult?> ActivateAsync(string licenseKey)
    {
        var request = new { licenseKey };
        return await PostAsync<ActivationResult>("license/activate", request);
    }
}

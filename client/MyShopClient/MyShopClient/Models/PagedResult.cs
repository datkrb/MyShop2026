using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MyShopClient.Models;

public class PagedResult<T>
{
    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = new();

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
}

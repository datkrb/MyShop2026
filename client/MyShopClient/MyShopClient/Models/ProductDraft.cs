using System.Collections.Generic;

namespace MyShopClient.Models;

public class ProductDraft
{
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal ImportPrice { get; set; }
    public decimal SalePrice { get; set; }
    public int CategoryId { get; set; }
    public int Stock { get; set; }
    // Only storing the first image URL for simplicity in draft, or list if needed
    public List<string> Images { get; set; } = new();
}

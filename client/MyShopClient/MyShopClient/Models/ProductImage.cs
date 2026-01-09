using Microsoft.UI.Xaml.Media.Imaging;

namespace MyShopClient.Models;

/// <summary>
/// Model for product images with source and file path
/// </summary>
public class ProductImage
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public BitmapImage? ImageSource { get; set; }
}

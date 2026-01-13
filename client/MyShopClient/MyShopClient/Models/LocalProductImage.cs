using Microsoft.UI.Xaml.Media.Imaging;

namespace MyShopClient.Models;

/// <summary>
/// Model for product images with source and file path (for upload purposes)
/// </summary>
public class LocalProductImage
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public BitmapImage? ImageSource { get; set; }
}

using System;
using Microsoft.UI.Xaml.Media;
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
    
    /// <summary>
    /// URL for remotely stored images (existing product images)
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Returns the best available image source (local BitmapImage preferred over remote URL)
    /// </summary>
    public ImageSource? DisplaySource => ImageSource ?? (ImageUrl != null ? new BitmapImage(new Uri(ImageUrl)) : null);
}


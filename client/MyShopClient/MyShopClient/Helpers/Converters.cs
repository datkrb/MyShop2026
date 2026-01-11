using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace MyShopClient.Helpers;

public class BoolToVisibilityConverter : IValueConverter
{
    public bool IsInverted { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool val)
        {
            if (IsInverted) val = !val;
            return val ? Visibility.Visible : Visibility.Collapsed;
        }
        return IsInverted ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class DecimalToDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal d)
        {
            return (double)d;
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is double d)
        {
            return (decimal)d;
        }
        return 0m;
    }
}

public class DoubleToNullableDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value ?? double.NaN;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is double d && !double.IsNaN(d))
        {
            return d;
        }
        return null;
    }
}

public class IntToDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int i) return (double)i;
        return double.NaN;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is double d && !double.IsNaN(d))
        {
            return (int)d;
        }
        
        // For nullable int, return null. For int, return 0.
        // targetType might be System.Nullable`1[System.Int32] or System.Int32
        if (targetType == typeof(int)) return 0;
        
        return null;
    }
}

public class IntToVisibleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int i && i > 0) return Visibility.Visible;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class StockToStatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int stock)
        {
            if (stock <= 0) return new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 235, 238)); // #FFEBEE (Light Red)
            if (stock < 10) return new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 248, 225)); // #FFF8E1 (Light Orange)
            return new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 232, 245, 233)); // #E8F5E9 (Light Green)
        }
        return new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class StockToStatusForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int stock)
        {
            if (stock <= 0) return new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 211, 47, 47)); // #D32F2F (Dark Red)
            if (stock < 10) return new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 143, 0)); // #FF8F00 (Dark Orange)
            return new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 56, 142, 60)); // #388E3C (Dark Green)
        }
        return new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Black);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class StockToStatusStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int stock)
        {
            if (stock <= 0) return "OutOfStock";
            if (stock < 10) return "LowStock";
            return "Published";
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ImageUrlConverter : IValueConverter
{
    private const string BaseUrl = "http://localhost:3000"; 

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        string? url = value as string;
        if (string.IsNullOrEmpty(url)) return null;

        string finalUrl = url;
        if (!url.StartsWith("http") && !url.StartsWith("ms-appx"))
        {
            if (url.StartsWith("/"))
                finalUrl = BaseUrl + url;
            else if (System.IO.Path.IsPathRooted(url))
                finalUrl = "file:///" + url.Replace("\\", "/");
            else
                finalUrl = BaseUrl + "/" + url;
        }

        try
        {
            return new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(finalUrl));
        }
        catch
        {
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}


public class ProductImageToSourceConverter : IValueConverter
{
    private readonly ImageUrlConverter _urlConverter = new();

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is System.Collections.IEnumerable list)
        {
            var enumerator = list.GetEnumerator();
            if (enumerator.MoveNext())
            {
               var firstItem = enumerator.Current;
               if (firstItem is MyShopClient.Models.ProductImage img)
               {
                   return _urlConverter.Convert(img.Url, targetType, parameter, language);
               }
            }
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System;
using Windows.UI;

namespace MyShopClient.Helpers;

/// <summary>
/// Converts bool to Brush for pagination button styling.
/// True = Primary color (current page), False = transparent/default
/// </summary>
public class BoolToPaginationBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isCurrentPage = value is bool b && b;
        string param = parameter as string ?? "";
        
        if (param == "Foreground")
        {
            // Text color
            return isCurrentPage 
                ? new SolidColorBrush(Colors.White) 
                : new SolidColorBrush(Color.FromArgb(255, 107, 114, 128)); // Gray text
        }
        
        // Background color
        if (isCurrentPage)
        {
            return new SolidColorBrush(Color.FromArgb(255, 124, 92, 252)); // #7C5CFC Primary
        }
        
        // Return a transparent brush to let the style's default background show through
        // If that doesn't work, we explicitly set a light card background
        return new SolidColorBrush(Color.FromArgb(255, 249, 250, 251)); // Light card bg #F9FAFB
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}


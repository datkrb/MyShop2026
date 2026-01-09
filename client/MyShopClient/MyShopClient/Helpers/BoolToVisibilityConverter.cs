using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace MyShopClient.Helpers;

/// <summary>
/// Converts bool to Visibility. True = Visible, False = Collapsed.
/// Use ConverterParameter = "Inverse" to invert the logic.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool boolValue = value is bool b && b;
        bool inverse = parameter is string s && s.Equals("Inverse", StringComparison.OrdinalIgnoreCase);

        if (inverse)
            boolValue = !boolValue;

        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        bool visibleValue = value is Visibility v && v == Visibility.Visible;
        bool inverse = parameter is string s && s.Equals("Inverse", StringComparison.OrdinalIgnoreCase);

        if (inverse)
            visibleValue = !visibleValue;

        return visibleValue;
    }
}

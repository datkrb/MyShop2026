using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System;

namespace MyShopClient.Helpers
{
    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var format = parameter as string;
            if (string.IsNullOrEmpty(format)) return value?.ToString() ?? string.Empty;
            try
            {
                // If parameter contains '{}' prefix (XAML literal escape), remove it
                if (format.StartsWith("{}")) format = format.Substring(2);
                return string.Format(format, value);
            }
            catch
            {
                return value?.ToString() ?? string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a boolean (isPositive) to a SolidColorBrush.
    /// True = Green (success), False = Red (danger)
    /// </summary>
    public class BoolToChangeBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush SuccessBrush = new(Windows.UI.Color.FromArgb(255, 34, 197, 94));
        private static readonly SolidColorBrush DangerBrush = new(Windows.UI.Color.FromArgb(255, 239, 68, 68));

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isPositive)
            {
                return isPositive ? SuccessBrush : DangerBrush;
            }
            return SuccessBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a boolean (isPositive) to an arrow glyph.
    /// True = Up arrow (E70D), False = Down arrow (E70E)
    /// </summary>
    public class BoolToChangeGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isPositive)
            {
                return !isPositive ? "\uE70D" : "\uE70E";
            }
            return "\uE70D";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
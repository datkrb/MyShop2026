using Microsoft.UI.Xaml.Data;
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
}
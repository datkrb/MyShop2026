using Microsoft.UI.Xaml.Data;
using System;

namespace MyShopClient.Helpers;

/// <summary>
/// Converter for formatting currency values in XAML bindings.
/// Converts numeric values to Vietnamese Dong format: "100.000 đ"
/// </summary>
public class CurrencyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null) return "0 đ";

        try
        {
            decimal amount = value switch
            {
                decimal d => d,
                double dbl => (decimal)dbl,
                float f => (decimal)f,
                int i => i,
                long l => l,
                _ => decimal.TryParse(value.ToString(), out var parsed) ? parsed : 0
            };

            return CurrencyHelper.FormatVND(amount);
        }
        catch
        {
            return "0 đ";
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

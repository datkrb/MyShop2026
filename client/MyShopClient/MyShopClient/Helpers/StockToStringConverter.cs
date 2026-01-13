using Microsoft.UI.Xaml.Data;
using System;

namespace MyShopClient.Helpers;

public class StockToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int stock)
        {
            return $"{stock} in stock";
        }
        return "0 in stock";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

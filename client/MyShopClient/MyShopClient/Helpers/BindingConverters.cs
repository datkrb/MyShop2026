using Microsoft.UI.Xaml.Data;
using System;

namespace MyShopClient.Helpers;

public class DateTimeToDateTimeOffsetConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime dt)
        {
            return new DateTimeOffset(dt);
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTimeOffset dto)
        {
            return dto.DateTime;
        }
        return DateTime.MinValue; // Or handle null
    }
}



public class NullableIntToDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
         if (value is int i) return (double)i;
         return double.NaN;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is double d)
        {
            if (double.IsNaN(d)) return null;
            return (int)d;
        }
        return null;
    }
}

using Microsoft.UI.Xaml.Data;
using System;

namespace MyShopClient.Helpers;

public class DateTimeFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime dt)
        {
            var format = parameter as string ?? "dd/MM/yyyy";
            return dt.ToString(format);
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

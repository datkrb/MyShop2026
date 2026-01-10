using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace MyShopClient.Helpers;

public class BoolToVisibilityConverter : IValueConverter
{
    public bool IsInverted { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool val = (bool)value;
        if (IsInverted) val = !val;
        return val ? Visibility.Visible : Visibility.Collapsed;
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

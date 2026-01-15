using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;
using MyShopClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MyShopClient.Views.Login;

public sealed partial class LoginView : Page
{
    public LoginViewModel ViewModel { get; }

    public LoginView()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetService<LoginViewModel>()!;
    }

    private void ConfigButton_Click(object sender, RoutedEventArgs e)
    {
        if (App.Current.RootFrame != null)
        {
            App.Current.RootFrame.Navigate(typeof(ServerConfigView));
        }
    }
}

public class StringToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return !string.IsNullOrEmpty(value as string);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class InvertBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return !(value is bool b && b);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return !(value is bool b && b);
    }
}

public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return !string.IsNullOrEmpty(value as string) 
            ? Microsoft.UI.Xaml.Visibility.Visible 
            : Microsoft.UI.Xaml.Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

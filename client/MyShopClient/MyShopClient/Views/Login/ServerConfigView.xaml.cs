using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using MyShopClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.UI;

namespace MyShopClient.Views.Login;

public sealed partial class ServerConfigView : Page
{
    public ServerConfigViewModel ViewModel { get; }

    public ServerConfigView()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetService<ServerConfigViewModel>()!;
    }

    private void BackToLogin_Click(object sender, RoutedEventArgs e)
    {
        if (App.Current.RootFrame != null)
        {
            App.Current.RootFrame.Navigate(typeof(LoginView));
        }
    }
}

/// <summary>
/// Converter to change text color based on success/failure status
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public string TrueColor { get; set; } = "#22c55e";
    public string FalseColor { get; set; } = "#ef4444";

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isSuccess = value is bool b && b;
        string colorHex = isSuccess ? TrueColor : FalseColor;
        
        // Parse hex color
        colorHex = colorHex.TrimStart('#');
        byte r = byte.Parse(colorHex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(colorHex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b2 = byte.Parse(colorHex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        
        return new SolidColorBrush(Color.FromArgb(255, r, g, b2));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

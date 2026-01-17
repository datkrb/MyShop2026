using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShopClient.Services.Api;
using MyShopClient.Services.Auth;

namespace MyShopClient.Views.License;

public sealed partial class ActivationView : Page
{
    public bool IsAdmin => App.Current.CurrentUserRole == "ADMIN";
    public bool IsSale => App.Current.CurrentUserRole == "SALE";

    public ActivationView()
    {
        this.InitializeComponent();
    }

    private async void ActivateButton_Click(object sender, RoutedEventArgs e)
    {
        var licenseKey = LicenseKeyTextBox.Text?.Trim();
        
        if (string.IsNullOrWhiteSpace(licenseKey))
        {
            ErrorText.Text = "Vui lòng nhập license key";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        try
        {
            ActivateButton.IsEnabled = false;
            ErrorText.Visibility = Visibility.Collapsed;

            var licenseService = App.Current.Services.GetService<LicenseApiService>();
            var result = await licenseService!.ActivateAsync(licenseKey);

            if (result?.Success == true)
            {
                // Navigate to ShellPage
                App.Current.RootFrame?.Navigate(typeof(ShellPage));
            }
            else
            {
                ErrorText.Text = result?.Message ?? "License key không hợp lệ";
                ErrorText.Visibility = Visibility.Visible;
            }
        }
        catch (System.Exception ex)
        {
            ErrorText.Text = $"Lỗi: {ex.Message}";
            ErrorText.Visibility = Visibility.Visible;
        }
        finally
        {
            ActivateButton.IsEnabled = true;
        }
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        // Clear credentials and navigate to login
        var credentialService = App.Current.Services.GetService<CredentialService>();
        credentialService?.ClearCredentials();
        
        App.Current.CurrentUserRole = string.Empty;
        App.Current.RootFrame?.Navigate(typeof(Views.Login.LoginView));
    }
}

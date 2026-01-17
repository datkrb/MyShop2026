using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShopClient.Services.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace MyShopClient.Views.Shared;

public sealed partial class PageHeader : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(PageHeader), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(PageHeader), new PropertyMetadata(string.Empty));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string UserName => App.Current.CurrentUserName ?? "User";
    public string UserRole => App.Current.CurrentUserRole ?? "Unknown";

    public PageHeader()
    {
        this.InitializeComponent();
    }

    private async void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        // Show confirmation dialog
        var dialog = new ContentDialog
        {
            Title = "Confirm Logout",
            Content = "Are you sure you want to logout?",
            PrimaryButtonText = "Logout",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            // Close the flyout
            UserFlyout.Hide();

            // Clear credentials
            var credentialService = App.Current.Services.GetService<CredentialService>();
            credentialService?.ClearCredentials();

            // Clear app state
            App.Current.CurrentUserName = string.Empty;
            App.Current.CurrentUserRole = string.Empty;

            // Navigate to login
            App.Current.RootFrame?.Navigate(typeof(Views.Login.LoginView));
        }
    }
}

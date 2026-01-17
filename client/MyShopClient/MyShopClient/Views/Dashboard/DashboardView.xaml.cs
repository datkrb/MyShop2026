using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShopClient.ViewModels;
using MyShopClient.Services.Api;
using System;

namespace MyShopClient.Views.Dashboard;

public sealed partial class DashboardView : Page
{
    public DashboardViewModel ViewModel { get; }

    public DashboardView()
    {
        this.InitializeComponent();
        
        ViewModel = App.Current.Services.GetService<DashboardViewModel>()!;
    }

    private async void ActivateButton_Click(object sender, RoutedEventArgs e)
    {
        // Tạo TextBox cho nhập license key
        var licenseKeyInput = new TextBox
        {
            PlaceholderText = "MYSH-XXXX-XXXX-XXXX",
            Header = "Nhập License Key:",
            Width = 300,
            CharacterCasing = CharacterCasing.Upper
        };

        var dialog = new ContentDialog
        {
            Title = "Kích hoạt License",
            Content = licenseKeyInput,
            PrimaryButtonText = "Kích hoạt",
            CloseButtonText = "Hủy",
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(licenseKeyInput.Text))
        {
            try
            {
                var licenseService = App.Current.Services.GetService<LicenseApiService>();
                var activationResult = await licenseService!.ActivateAsync(licenseKeyInput.Text.Trim());

                if (activationResult?.Success == true)
                {
                    // Ẩn trial banner
                    ViewModel.ShowTrialBanner = false;
                    ViewModel.IsActivated = true;

                    await new ContentDialog
                    {
                        Title = "Thành công",
                        Content = "License đã được kích hoạt thành công!",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    }.ShowAsync();
                }
                else
                {
                    await new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = activationResult?.Message ?? "License key không hợp lệ",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    }.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                await new ContentDialog
                {
                    Title = "Lỗi",
                    Content = $"Không thể kích hoạt: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                }.ShowAsync();
            }
        }
    }
}

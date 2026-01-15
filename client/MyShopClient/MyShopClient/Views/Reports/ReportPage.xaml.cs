using Microsoft.UI.Xaml.Controls;
using MyShopClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace MyShopClient.Views.Reports;

public sealed partial class ReportPage : Page
{
    public ReportViewModel ViewModel { get; }

    public ReportPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetService<ReportViewModel>();
    }
}

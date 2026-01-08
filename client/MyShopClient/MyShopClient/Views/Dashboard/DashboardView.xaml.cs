using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MyShopClient.ViewModels;

namespace MyShopClient.Views.Dashboard;

public sealed partial class DashboardView : Page
{
    public DashboardViewModel ViewModel { get; }

    public DashboardView()
    {
        this.InitializeComponent();
        
        ViewModel = App.Current.Services.GetService<DashboardViewModel>()!;
    }
}

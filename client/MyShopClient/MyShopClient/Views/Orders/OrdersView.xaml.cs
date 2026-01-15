using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShopClient.Services.Navigation;
using MyShopClient.ViewModels;

namespace MyShopClient.Views.Orders;

public sealed partial class OrdersView : Page
{
    public OrdersViewModel ViewModel { get; }
    private readonly INavigationService _navigationService;

    public OrdersView()
    {
        this.InitializeComponent();
        _navigationService = App.Current.Services.GetRequiredService<INavigationService>();
        ViewModel = App.Current.Services.GetService<OrdersViewModel>()!;
    }
    
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadOrdersAsync();
    }
    
    private void OrdersListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is OrderViewModel order)
        {
            _navigationService.Navigate(typeof(OrderDetailView), order.Id);
        }
    }
    
    private void CreateOrderButton_Click(object sender, RoutedEventArgs e)
    {
        _navigationService.Navigate(typeof(OrderDetailView), "new");
    }
    
    private void OnPageChanged(object sender, int pageNumber)
    {
        _ = ViewModel.GoToPageAsync(pageNumber);
    }
}

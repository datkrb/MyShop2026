using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShopClient.ViewModels;

namespace MyShopClient.Views.Orders;

public sealed partial class OrdersView : Page
{
    public OrdersViewModel ViewModel { get; }

    public OrdersView()
    {
        this.InitializeComponent();
        
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
            Frame.Navigate(typeof(OrderDetailView), order.Id);
        }
    }
    
    private void CreateOrderButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(OrderDetailView), "new");
    }
    
    private void OnPageChanged(object sender, int pageNumber)
    {
        _ = ViewModel.GoToPageAsync(pageNumber);
    }
}

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShopClient.ViewModels;

namespace MyShopClient.Views.Orders;

public sealed partial class OrderDetailView : Page
{
    public OrderDetailViewModel ViewModel { get; } = new OrderDetailViewModel();

    public OrderDetailView()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is string param && param == "new")
        {
            ViewModel.InitializeNewOrder();
        }
        else if (e.Parameter is OrderViewModel order)
        {
            ViewModel.LoadOrder(order);
        }
        else if (e.Parameter is int orderId)
        {
             // TODO: Load by ID if just ID passed
             // ViewModel.LoadOrderById(orderId);
        }
    }

    private void BackButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private void EditStatusButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Handled by Command in ViewModel usually, but if button click handler exists:
        // ViewModel.EditStatus();
        // Since we are moving to Command binding, this might not be needed if XAML updates
    }

    private void DeleteOrderButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
         // ViewModel.DeleteOrderCommand.Execute(null);
    }
}

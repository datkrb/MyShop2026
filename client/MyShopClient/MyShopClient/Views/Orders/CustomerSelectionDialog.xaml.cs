using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShopClient.ViewModels;

namespace MyShopClient.Views.Orders;

public sealed partial class CustomerSelectionDialog : ContentDialog
{
    public CustomerSelectionViewModel ViewModel
    {
        get => (CustomerSelectionViewModel)DataContext;
        set => DataContext = value;
    }

    public CustomerSelectionDialog()
    {
        this.InitializeComponent();
    }

    private void SearchBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
             ViewModel.SearchCommand.Execute(null);
        }
    }
}

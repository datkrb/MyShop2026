using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MyShopClient.ViewModels;

namespace MyShopClient.Views.Orders;

public sealed partial class CreateOrderDialog : ContentDialog
{
    public CreateOrderDialogViewModel ViewModel { get; }

    public CreateOrderDialog()
    {
        this.InitializeComponent();
        
        ViewModel = App.Current.Services.GetService<CreateOrderDialogViewModel>() 
            ?? new CreateOrderDialogViewModel();
        
        // Subscribe to close request
        ViewModel.DialogCloseRequested += OnDialogCloseRequested;
        
        // Set DataContext for remove button binding
        OrderItemsListView.DataContext = this;
    }

    private void OnDialogCloseRequested(object? sender, bool result)
    {
        Hide();
    }
}

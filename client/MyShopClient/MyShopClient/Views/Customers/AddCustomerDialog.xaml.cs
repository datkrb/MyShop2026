using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MyShopClient.ViewModels;

namespace MyShopClient.Views.Customers;

public sealed partial class AddCustomerDialog : ContentDialog
{
    public AddCustomerDialogViewModel ViewModel { get; }

    public AddCustomerDialog()
    {
        this.InitializeComponent();
        
        ViewModel = App.Current.Services.GetService<AddCustomerDialogViewModel>() 
            ?? new AddCustomerDialogViewModel();
        
        this.DataContext = this;
        
        ViewModel.DialogCloseRequested += (s, success) =>
        {
            this.Hide();
        };
    }
}

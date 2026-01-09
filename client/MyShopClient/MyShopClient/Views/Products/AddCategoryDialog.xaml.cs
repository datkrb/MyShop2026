using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MyShopClient.ViewModels;

namespace MyShopClient.Views.Products;

public sealed partial class AddCategoryDialog : ContentDialog
{
    public AddCategoryDialogViewModel ViewModel { get; }

    public AddCategoryDialog()
    {
        this.InitializeComponent();
        
        ViewModel = App.Current.Services.GetService<AddCategoryDialogViewModel>() 
            ?? new AddCategoryDialogViewModel();
        
        // Subscribe to close request
        ViewModel.DialogCloseRequested += OnDialogCloseRequested;
    }

    private void OnDialogCloseRequested(object? sender, bool result)
    {
        Hide();
    }
}

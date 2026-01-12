using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MyShopClient.ViewModels;
using System;

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
    }

    private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Get deferral to prevent dialog from closing immediately
        var deferral = args.GetDeferral();
        
        try
        {
            // Call the save method which returns true if successful
            var success = await ViewModel.SaveAsync();
            
            if (!success)
            {
                // Cancel the close action - dialog stays open with error message
                args.Cancel = true;
            }
            // If success is true, dialog will close normally
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving: {ex.Message}");
            args.Cancel = true;
        }
        finally
        {
            deferral.Complete();
        }
    }

    private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Just close the dialog - no action needed
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MyShopClient.ViewModels;
using System;

namespace MyShopClient.Views.Employees;

public sealed partial class AddEmployeeDialog : ContentDialog
{
    public AddEmployeeDialogViewModel ViewModel { get; }

    public string PasswordPlaceholder => ViewModel.IsEditMode 
        ? "Enter new password (optional)..." 
        : "Enter password (min 6 characters)...";

    public AddEmployeeDialog()
    {
        this.InitializeComponent();
        
        ViewModel = App.Current.Services.GetService<AddEmployeeDialogViewModel>() 
            ?? new AddEmployeeDialogViewModel(App.Current.Services.GetRequiredService<MyShopClient.Services.Api.EmployeeApiService>());
        
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

    // Static helper methods for ComboBox ItemTemplate
    public static string GetRoleIcon(string role)
    {
        return role switch
        {
            "ADMIN" => "\uE7EE",
            "SALE" => "\uE77B",
            _ => "\uE77B"
        };
    }

    public static string GetRoleDisplayName(string role)
    {
        return role switch
        {
            "ADMIN" => "Administrator",
            "SALE" => "Sales Staff",
            _ => role
        };
    }
}

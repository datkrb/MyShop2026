using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShopClient.Models;
using MyShopClient.ViewModels;
using System;
using System.Linq;

namespace MyShopClient.Views.Employees;

public sealed partial class EmployeesView : Page
{
    public EmployeesViewModel ViewModel { get; }

    public EmployeesView()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetService<EmployeesViewModel>()!;
    }
    
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadEmployeesAsync();
    }
    
    private async void AddEmployeeButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddEmployeeDialog
        {
            XamlRoot = this.XamlRoot
        };
        
        dialog.ViewModel.Reset();
        
        var result = await dialog.ShowAsync();
        
        if (result == ContentDialogResult.Primary)
        {
            Notification.ShowSuccess("Employee created successfully!");
            await ViewModel.LoadEmployeesAsync();
        }
    }
    
    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int employeeId)
        {
            var employee = ViewModel.Employees.FirstOrDefault(emp => emp.Id == employeeId);
            if (employee == null) return;

            var dialog = new AddEmployeeDialog
            {
                XamlRoot = this.XamlRoot
            };
            
            dialog.ViewModel.SetEditMode(new User 
            { 
                Id = employee.Id, 
                Username = employee.Username, 
                Role = employee.Role 
            });
            
            var result = await dialog.ShowAsync();
            
            if (result == ContentDialogResult.Primary)
            {
                Notification.ShowSuccess("Employee updated successfully!");
                await ViewModel.LoadEmployeesAsync();
            }
        }
    }
    
    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int employeeId)
        {
            var employee = ViewModel.Employees.FirstOrDefault(emp => emp.Id == employeeId);
            if (employee == null) return;

            var dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "Confirm Delete",
                Content = $"Are you sure you want to delete employee \"{employee.Username}\"?\nThis action cannot be undone.",
                PrimaryButtonText = "Delete",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Secondary
            };
            
            var result = await dialog.ShowAsync();
            
            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.DeleteEmployeeAsync(employeeId);
                
                if (string.IsNullOrEmpty(ViewModel.ErrorMessage))
                {
                    Notification.ShowSuccess("Employee deleted successfully!");
                }
                else
                {
                    Notification.ShowError(ViewModel.ErrorMessage);
                }
            }
        }
    }
    
    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.RefreshAsync();
    }
}

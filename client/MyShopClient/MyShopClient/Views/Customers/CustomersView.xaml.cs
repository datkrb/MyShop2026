using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MyShopClient.ViewModels;
using System;
using Windows.UI;

namespace MyShopClient.Views.Customers;

public sealed partial class CustomersView : Page
{
    public CustomersViewModel ViewModel { get; }
    
    private static readonly SolidColorBrush PrimaryBrush = new(Color.FromArgb(255, 124, 92, 252));
    private static readonly SolidColorBrush WhiteBrush = new(Colors.White);
    private static readonly SolidColorBrush GrayBrush = new(Color.FromArgb(255, 107, 114, 128));

    public CustomersView()
    {
        this.InitializeComponent();
        
        ViewModel = App.Current.Services.GetService<CustomersViewModel>()!;
        
        ViewModel.PageNumbers.CollectionChanged += (s, e) => UpdatePageButtonStyles();
    }
    
    private void PageButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int pageNumber)
        {
            ViewModel.GoToPageCommand.Execute(pageNumber);
        }
    }
    
    private void CustomersListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is CustomerViewModel customer)
        {
            Frame.Navigate(typeof(CustomerDetailView), customer);
        }
    }
    
    private async void AddCustomerButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddCustomerDialog
        {
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
        
        ViewModel.RefreshCommand.Execute(null);
    }
    
    private async void EditCustomerButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is CustomerViewModel customer)
        {
            var dialog = new AddCustomerDialog
            {
                XamlRoot = this.XamlRoot
            };
            
            dialog.ViewModel.LoadCustomer(customer);
            
            await dialog.ShowAsync();
            
            ViewModel.RefreshCommand.Execute(null);
        }
    }
    
    private async void DeleteCustomerButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is CustomerViewModel customer)
        {
            var confirmDialog = new ContentDialog
            {
                Title = "Delete Customer",
                Content = $"Are you sure you want to delete {customer.Name}? This action cannot be undone.",
                PrimaryButtonText = "Delete",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                ViewModel.RefreshCommand.Execute(null);
            }
        }
    }
    
    private void UpdatePageButtonStyles()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (PageNumbersControl?.ItemsPanelRoot == null) return;
            
            foreach (var child in PageNumbersControl.ItemsPanelRoot.Children)
            {
                if (child is ContentPresenter presenter && presenter.Content is PageButtonModel model)
                {
                    var button = FindChild<Button>(presenter);
                    if (button != null)
                    {
                        ApplyButtonStyle(button, model.IsCurrentPage);
                    }
                }
                else if (child is Button button && button.DataContext is PageButtonModel model2)
                {
                    ApplyButtonStyle(button, model2.IsCurrentPage);
                }
            }
        });
    }
    
    private void ApplyButtonStyle(Button button, bool isCurrentPage)
    {
        if (isCurrentPage)
        {
            button.Background = PrimaryBrush;
            button.Foreground = WhiteBrush;
        }
        else
        {
            button.Background = null;
            button.Foreground = GrayBrush;
        }
    }
    
    private static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
    {
        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typedChild)
                return typedChild;
            var found = FindChild<T>(child);
            if (found != null)
                return found;
        }
        return null;
    }
}

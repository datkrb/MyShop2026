using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MyShopClient.ViewModels;
using System;
using Windows.UI;

namespace MyShopClient.Views.Orders;

public sealed partial class OrdersView : Page
{
    public OrdersViewModel ViewModel { get; }
    
    private static readonly SolidColorBrush PrimaryBrush = new(Color.FromArgb(255, 124, 92, 252));
    private static readonly SolidColorBrush WhiteBrush = new(Colors.White);
    private static readonly SolidColorBrush GrayBrush = new(Color.FromArgb(255, 107, 114, 128));

    public OrdersView()
    {
        this.InitializeComponent();
        
        ViewModel = App.Current.Services.GetService<OrdersViewModel>()!;
        
        ViewModel.PageNumbers.CollectionChanged += (s, e) => UpdatePageButtonStyles();
    }
    
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadOrdersAsync();
    }
    
    private void PageButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int pageNumber)
        {
            _ = ViewModel.GoToPageAsync(pageNumber);
        }
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
    
    private void EditOrderButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is OrderViewModel order)
        {
            Frame.Navigate(typeof(OrderDetailView), order.Id);
        }
    }
    
    private async void DeleteOrderButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is OrderViewModel order)
        {
            var confirmDialog = new ContentDialog
            {
                Title = "Delete Order",
                Content = $"Are you sure you want to delete order {order.OrderId}? This action cannot be undone.",
                PrimaryButtonText = "Delete",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.DeleteOrderAsync(order);
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
                    var foundButton = FindChild<Button>(presenter);
                    if (foundButton != null)
                    {
                        ApplyButtonStyle(foundButton, model.IsCurrentPage);
                    }
                }
                else if (child is Button btn && btn.DataContext is PageButtonModel model2)
                {
                    ApplyButtonStyle(btn, model2.IsCurrentPage);
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

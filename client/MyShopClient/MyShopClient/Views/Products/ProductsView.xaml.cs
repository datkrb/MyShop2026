using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MyShopClient.ViewModels;
using Windows.UI;

namespace MyShopClient.Views.Products;

public sealed partial class ProductsView : Page
{
    public ProductsViewModel ViewModel { get; }
    
    // Primary color brush for current page
    private static readonly SolidColorBrush PrimaryBrush = new(Color.FromArgb(255, 124, 92, 252));
    private static readonly SolidColorBrush WhiteBrush = new(Colors.White);
    private static readonly SolidColorBrush GrayBrush = new(Color.FromArgb(255, 107, 114, 128));

    public ProductsView()
    {
        this.InitializeComponent();
        
        ViewModel = App.Current.Services.GetService<ProductsViewModel>()!;
        
        // Subscribe to PageNumbers collection changes to update styling
        ViewModel.PageNumbers.CollectionChanged += (s, e) => UpdatePageButtonStyles();
    }
    
    private void PageButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int pageNumber)
        {
            ViewModel.GoToPageCommand.Execute(pageNumber);
            // Styling will be updated via CollectionChanged event
        }
    }
    
    private void UpdatePageButtonStyles()
    {
        // Use DispatcherQueue to ensure UI is updated
        DispatcherQueue.TryEnqueue(() =>
        {
            if (PageNumbersControl?.ItemsPanelRoot == null) return;
            
            foreach (var child in PageNumbersControl.ItemsPanelRoot.Children)
            {
                if (child is ContentPresenter presenter && presenter.Content is PageButtonModel model)
                {
                    // Find the Button inside the ContentPresenter
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
            button.Background = null; // Use default from style
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


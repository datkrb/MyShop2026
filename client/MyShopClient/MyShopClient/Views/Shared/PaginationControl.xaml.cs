using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MyShopClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI;

namespace MyShopClient.Views.Shared;

/// <summary>
/// Reusable pagination control with page numbers and navigation buttons
/// </summary>
public sealed partial class PaginationControl : UserControl
{
    private static readonly SolidColorBrush PrimaryBrush = new(Color.FromArgb(255, 124, 92, 252));
    private static readonly SolidColorBrush WhiteBrush = new(Colors.White);
    private static readonly SolidColorBrush GrayBrush = new(Color.FromArgb(255, 107, 114, 128));
    private static readonly SolidColorBrush TransparentBrush = new(Colors.Transparent);

    #region Dependency Properties

    public static readonly DependencyProperty CurrentPageProperty =
        DependencyProperty.Register(nameof(CurrentPage), typeof(int), typeof(PaginationControl),
            new PropertyMetadata(1, OnPaginationPropertyChanged));

    public static readonly DependencyProperty TotalPagesProperty =
        DependencyProperty.Register(nameof(TotalPages), typeof(int), typeof(PaginationControl),
            new PropertyMetadata(1, OnPaginationPropertyChanged));

    public static readonly DependencyProperty TotalItemsProperty =
        DependencyProperty.Register(nameof(TotalItems), typeof(int), typeof(PaginationControl),
            new PropertyMetadata(0, OnPaginationPropertyChanged));

    public static readonly DependencyProperty PageSizeProperty =
        DependencyProperty.Register(nameof(PageSize), typeof(int), typeof(PaginationControl),
            new PropertyMetadata(10, OnPaginationPropertyChanged));

    public static readonly DependencyProperty PageNumbersProperty =
        DependencyProperty.Register(nameof(PageNumbers), typeof(ObservableCollection<PageButtonModel>), typeof(PaginationControl),
            new PropertyMetadata(null, OnPageNumbersChanged));

    public static readonly DependencyProperty ItemsLabelProperty =
        DependencyProperty.Register(nameof(ItemsLabel), typeof(string), typeof(PaginationControl),
            new PropertyMetadata("items", OnItemsLabelChanged));

    public static readonly DependencyProperty PageSizeOptionsProperty =
        DependencyProperty.Register(nameof(PageSizeOptions), typeof(IList<int>), typeof(PaginationControl),
            new PropertyMetadata(new List<int> { 5, 10, 15, 20 }, OnPageSizeOptionsChanged));

    public int CurrentPage
    {
        get => (int)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public int TotalPages
    {
        get => (int)GetValue(TotalPagesProperty);
        set => SetValue(TotalPagesProperty, value);
    }

    public int TotalItems
    {
        get => (int)GetValue(TotalItemsProperty);
        set => SetValue(TotalItemsProperty, value);
    }

    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    public ObservableCollection<PageButtonModel> PageNumbers
    {
        get => (ObservableCollection<PageButtonModel>)GetValue(PageNumbersProperty);
        set => SetValue(PageNumbersProperty, value);
    }

    public string ItemsLabel
    {
        get => (string)GetValue(ItemsLabelProperty);
        set => SetValue(ItemsLabelProperty, value);
    }

    public IList<int> PageSizeOptions
    {
        get => (IList<int>)GetValue(PageSizeOptionsProperty);
        set => SetValue(PageSizeOptionsProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<int>? PageChanged;
    public event EventHandler<int>? PageSizeChanged;

    #endregion

    public PaginationControl()
    {
        this.InitializeComponent();
        // Initialize with default page size options
        PageSizeComboBox.ItemsSource = PageSizeOptions;
        PageSizeComboBox.SelectedItem = PageSize;
    }

    private static void OnPaginationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationControl control)
        {
            control.UpdateUI();
        }
    }

    private static void OnPageNumbersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationControl control)
        {
            // Unsubscribe from old collection
            if (e.OldValue is ObservableCollection<PageButtonModel> oldCollection)
            {
                oldCollection.CollectionChanged -= control.PageNumbers_CollectionChanged;
            }

            // Subscribe to new collection
            if (e.NewValue is ObservableCollection<PageButtonModel> newCollection)
            {
                newCollection.CollectionChanged += control.PageNumbers_CollectionChanged;
                control.PageNumbersControl.ItemsSource = newCollection;
                control.UpdatePageButtonStyles();
            }
        }
    }

    private static void OnItemsLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationControl control && e.NewValue is string label)
        {
            control.ItemsLabelRun.Text = label;
        }
    }

    private static void OnPageSizeOptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationControl control && e.NewValue is IList<int> options)
        {
            control.PageSizeComboBox.ItemsSource = options;
            if (options.Count > 0 && !options.Contains(control.PageSize))
            {
                control.PageSizeComboBox.SelectedItem = options[0];
            }
            else
            {
                control.PageSizeComboBox.SelectedItem = control.PageSize;
            }
        }
    }

    private void PageNumbers_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdatePageButtonStyles();
    }

    private void UpdateUI()
    {
        // Update page info text
        int pageStart = TotalItems > 0 ? (CurrentPage - 1) * PageSize + 1 : 0;
        int pageEnd = Math.Min(CurrentPage * PageSize, TotalItems);
        
        PageStartRun.Text = pageStart.ToString();
        PageEndRun.Text = pageEnd.ToString();
        TotalItemsRun.Text = TotalItems.ToString();

        // Update button states
        PreviousButton.IsEnabled = CurrentPage > 1;
        NextButton.IsEnabled = CurrentPage < TotalPages;

        UpdatePageButtonStyles();
    }

    private void UpdatePageButtonStyles()
    {
        DispatcherQueue.TryEnqueue(async () =>
        {
            await Task.Delay(50);
            
            if (PageNumbersControl?.ItemsPanelRoot == null) return;
            
            foreach (var child in PageNumbersControl.ItemsPanelRoot.Children)
            {
                Button? button = null;
                PageButtonModel? model = null;

                if (child is ContentPresenter presenter)
                {
                    model = presenter.Content as PageButtonModel;
                    button = FindChild<Button>(presenter);
                }
                else if (child is Button btn)
                {
                    button = btn;
                    model = btn.DataContext as PageButtonModel;
                }

                if (button != null && model != null)
                {
                    ApplyButtonStyle(button, model.IsCurrentPage);
                    
                    // Wire up click handler
                    button.Click -= PageButton_Click;
                    button.Click += PageButton_Click;
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
            button.BorderBrush = PrimaryBrush;
        }
        else
        {
            button.Background = TransparentBrush;
            button.Foreground = GrayBrush;
            button.ClearValue(Control.BorderBrushProperty);
        }
    }

    private void PageButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int pageNumber && pageNumber != CurrentPage)
        {
            PageChanged?.Invoke(this, pageNumber);
        }
    }

    private void PreviousButton_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentPage > 1)
        {
            PageChanged?.Invoke(this, CurrentPage - 1);
        }
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentPage < TotalPages)
        {
            PageChanged?.Invoke(this, CurrentPage + 1);
        }
    }

    private void PageSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PageSizeComboBox.SelectedItem is int selectedSize)
        {
            PageSizeChanged?.Invoke(this, selectedSize);
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

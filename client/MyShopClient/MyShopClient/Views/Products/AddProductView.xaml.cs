using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShopClient.ViewModels;
using MyShopClient.Models;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace MyShopClient.Views.Products;

public sealed partial class AddProductView : Page
{
    public AddProductViewModel ViewModel { get; }

    public AddProductView()
    {
        this.InitializeComponent();
        ViewModel = new AddProductViewModel();
        this.DataContext = this;
        
        // Subscribe to dialog close event for navigation
        ViewModel.DialogCloseRequested += ViewModel_DialogCloseRequested;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // If parameter is a product ID, load the product for editing
        if (e.Parameter is int productId && productId > 0)
        {
            await ViewModel.LoadProductAsync(productId);
        }
    }

    private void ViewModel_DialogCloseRequested(object? sender, bool success)
    {
        if (success)
        {
            var message = ViewModel.IsEditMode ? "Product updated successfully!" : "Product created successfully!";
            Notification.ShowSuccess(message);
        }
        
        // Navigate back
        if (App.Current.ContentFrame?.CanGoBack == true)
        {
            App.Current.ContentFrame.GoBack();
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (App.Current.ContentFrame?.CanGoBack == true)
        {
            App.Current.ContentFrame.GoBack();
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Trigger save command
        if (ViewModel.SaveCommand.CanExecute(null))
        {
            await ViewModel.SaveCommand.ExecuteAsync(null);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        if (App.Current.ContentFrame?.CanGoBack == true)
        {
            App.Current.ContentFrame.GoBack();
        }
    }

    private void DropZone_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
        
        if (e.DragUIOverride != null)
        {
            e.DragUIOverride.Caption = "Drop to add images";
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.IsGlyphVisible = true;
        }
        
        // Visual feedback
        if (sender is Border border)
        {
            border.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["PrimaryBrush"];
        }
    }

    private void DropZone_DragLeave(object sender, DragEventArgs e)
    {
        // Reset visual feedback
        if (sender is Border border)
        {
            border.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["BorderBrush"];
        }
    }

    private async void DropZone_Drop(object sender, DragEventArgs e)
    {
        // Reset visual feedback
        if (sender is Border border)
        {
            border.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["BorderBrush"];
        }

        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            
            foreach (var item in items)
            {
                if (item is StorageFile file)
                {
                    var ext = file.FileType.ToLowerInvariant();
                    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".bmp" || ext == ".webp")
                    {
                        await ViewModel.AddImageFromFileAsync(file);
                    }
                }
            }
        }
    }

    private async void DropZone_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        // Open file picker
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Current.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".gif");
        picker.FileTypeFilter.Add(".bmp");
        picker.FileTypeFilter.Add(".webp");

        var files = await picker.PickMultipleFilesAsync();
        if (files != null && files.Count > 0)
        {
            foreach (var file in files)
            {
                await ViewModel.AddImageFromFileAsync(file);
            }
        }
    }

    private void RemoveImage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is LocalProductImage image)
        {
            ViewModel.RemoveImageCommand.Execute(image);
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MyShopClient.ViewModels;
using System;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace MyShopClient.Views.Products;

public sealed partial class AddProductDialog : ContentDialog
{
    public AddProductDialogViewModel ViewModel { get; }

    public AddProductDialog()
    {
        this.InitializeComponent();
        
        ViewModel = App.Current.Services.GetService<AddProductDialogViewModel>() 
            ?? new AddProductDialogViewModel();
        
        // Subscribe to close request
        ViewModel.DialogCloseRequested += OnDialogCloseRequested;
        
        // Set DataContext for remove button binding
        ImagesGridView.DataContext = this;
    }

    private void OnDialogCloseRequested(object? sender, bool result)
    {
        if (result)
        {
            Hide();
        }
        else
        {
            Hide();
        }
    }

    private void DropZone_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
        
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            ViewModel.IsDragOver = true;
            e.DragUIOverride.Caption = "Thả để thêm ảnh";
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.IsGlyphVisible = true;
        }
    }

    private void DropZone_DragLeave(object sender, DragEventArgs e)
    {
        ViewModel.IsDragOver = false;
    }

    private async void DropZone_Drop(object sender, DragEventArgs e)
    {
        ViewModel.IsDragOver = false;
        
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            
            foreach (var item in items)
            {
                if (item is StorageFile file)
                {
                    // Check if it's a supported image file
                    var supportedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                    var extension = file.FileType.ToLower();
                    
                    if (supportedExtensions.Contains(extension))
                    {
                        await ViewModel.AddImageFromFileAsync(file);
                    }
                }
            }
        }
    }

    private async void DropZone_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        // Trigger file picker when drop zone is clicked
        await ViewModel.AddImagesCommand.ExecuteAsync(null);
    }
}

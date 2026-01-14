using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MyShopClient.ViewModels;
using MyShopClient.Services.Api;
using MyShopClient.Models;

namespace MyShopClient.Views.Products;

public sealed partial class ProductsView : Page
{
    public ProductViewModel ViewModel { get; }

    public ProductsView()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<ProductViewModel>();
        this.DataContext = ViewModel;
        this.Loaded += ProductsView_Loaded;
    }

    private async void ProductsView_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadDataAsync();
    }

    private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        ViewModel.SearchCommand.Execute(null);
    }

    private void ProductsListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is ApiProduct product)
        {
            // Navigate to detail view
            App.Current.ContentFrame?.Navigate(typeof(ProductDetailView), product.Id);
        }
    }

    private void AddProductButton_Click(object sender, RoutedEventArgs e)
    {
        // Navigate to add product view (using detail view with ID = 0 for create mode)
        App.Current.ContentFrame?.Navigate(typeof(AddProductView));
    }

    private async void AddCategory_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddCategoryDialog
        {
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(dialog.CategoryName))
        {
            var newCat = await ProductApiService.Instance.CreateCategoryAsync(dialog.CategoryName, dialog.CategoryDescription);
            if (newCat != null)
            {
                Notification.ShowSuccess("Category created successfully!");
                await ViewModel.LoadDataAsync();
            }
            else
            {
                Notification.ShowError("Failed to create category.");
            }
        }
    }

    private async void ImportProducts_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".xlsx");
            picker.FileTypeFilter.Add(".xls");
            picker.FileTypeFilter.Add(".accdb");
            picker.FileTypeFilter.Add(".mdb");

            // Get the current window handle for the picker
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Current.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file == null) return;

            // Show loading
            ViewModel.IsLoading = true;

            var importService = new MyShopClient.Services.Import.ImportService();
            System.Collections.Generic.List<ApiProduct> products;

            var extension = System.IO.Path.GetExtension(file.Path).ToLower();
            if (extension == ".xlsx" || extension == ".xls")
            {
                products = await importService.ImportFromExcelAsync(file.Path);
            }
            else if (extension == ".accdb" || extension == ".mdb")
            {
                products = await importService.ImportFromAccessAsync(file.Path);
            }
            else
            {
                Notification.ShowError("Unsupported file format. Please use Excel (.xlsx, .xls) or Access (.accdb, .mdb)");
                return;
            }

            if (products == null || products.Count == 0)
            {
                Notification.ShowWarning("No products found in the file.");
                ViewModel.IsLoading = false;
                return;
            }

            // Import products to server
            int newProductCount = 0;
            int existingProductCount = 0;
            int failCount = 0;
            
            var newProductSkus = new System.Collections.Generic.List<string>();
            var existingProductSkus = new System.Collections.Generic.List<string>();
            var failedProductSkus = new System.Collections.Generic.List<string>();

            foreach (var product in products)
            {
                try
                {
                    var created = await ProductApiService.Instance.CreateProductAsync(product);
                    
                    if (created != null)
                    {
                        // Check if product has image paths and upload them
                        if (!string.IsNullOrEmpty(product.Description) && product.Description.Contains("IMAGE:"))
                        {
                            // Extract image paths from description (format: "Description text IMAGE:path1;path2;path3")
                            var parts = product.Description.Split(new[] { "IMAGE:" }, StringSplitOptions.None);
                            if (parts.Length > 1)
                            {
                                var imagePaths = parts[1].Split(';')
                                    .Select(p => p.Trim())
                                    .Where(p => !string.IsNullOrEmpty(p) && System.IO.File.Exists(p))
                                    .ToList();

                                if (imagePaths.Any())
                                {
                                    await ProductApiService.Instance.UploadProductImagesAsync(created.Id, imagePaths);
                                }

                                // Clean description (remove IMAGE: part)
                                created.Description = parts[0].Trim();
                                await ProductApiService.Instance.UpdateProductAsync(created.Id, created);
                            }
                        }
                        
                        newProductCount++;
                        newProductSkus.Add(product.Sku ?? "N/A");
                    }
                    else
                    {
                        failCount++;
                        failedProductSkus.Add(product.Sku ?? "N/A");
                    }
                }
                catch (Exception ex)
                {
                    // Check if it's a duplicate SKU error (product already exists)
                    if (ex.Message.Contains("SKU") || ex.Message.Contains("already exists") || ex.Message.Contains("duplicate"))
                    {
                        existingProductCount++;
                        existingProductSkus.Add(product.Sku ?? "N/A");
                    }
                    else
                    {
                        failCount++;
                        failedProductSkus.Add(product.Sku ?? "N/A");
                    }
                }
            }

            ViewModel.IsLoading = false;
            
            // Build detailed message
            var messageLines = new System.Collections.Generic.List<string>();
            messageLines.Add($"Import completed: {products.Count} product(s) processed");
            messageLines.Add("");
            
            if (newProductCount > 0)
            {
                messageLines.Add($"✅ New products created: {newProductCount}");
                messageLines.Add($"   SKUs: {string.Join(", ", newProductSkus)}");
                messageLines.Add("");
            }
            
            if (existingProductCount > 0)
            {
                messageLines.Add($"⚠️ Existing products (skipped): {existingProductCount}");
                messageLines.Add($"   SKUs: {string.Join(", ", existingProductSkus)}");
                messageLines.Add("");
            }
            
            if (failCount > 0)
            {
                messageLines.Add($"❌ Failed to import: {failCount}");
                messageLines.Add($"   SKUs: {string.Join(", ", failedProductSkus)}");
            }

            var fullMessage = string.Join("\n", messageLines);
            
            // Show appropriate notification based on results
            // Show result dialog
            var dialog = new ContentDialog
            {
                Title = "Import Results",
                Content = new ScrollViewer 
                { 
                    Content = new TextBlock 
                    { 
                        Text = fullMessage, 
                        TextWrapping = TextWrapping.Wrap 
                    },
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    MaxHeight = 400
                },
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();

            // Refresh list if any product was added
            if (newProductCount > 0)
            {
                await ViewModel.LoadDataAsync();
            }
        }
        catch (Exception ex)
        {
            ViewModel.IsLoading = false;
            Notification.ShowError($"Import error: {ex.Message}");
        }
    }

    private async void OnPageChanged(object sender, int page)
    {
        await ViewModel.GoToPageAsync(page);
    }
}

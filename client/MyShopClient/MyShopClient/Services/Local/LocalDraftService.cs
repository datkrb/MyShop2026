using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MyShopClient.Models;

namespace MyShopClient.Services.Local;

public interface ILocalDraftService
{
    Task SaveDraftAsync(OrderDraft draft);
    Task<OrderDraft?> GetDraftAsync();
    Task ClearDraftAsync();
    
    // Product Drafts
    Task SaveProductDraftAsync(ProductDraft draft);
    Task<ProductDraft?> GetProductDraftAsync();
    Task ClearProductDraftAsync();
}

public class LocalDraftService : ILocalDraftService
{
    private const string DRAFT_FILENAME = "order_draft.json";
    private const string PRODUCT_DRAFT_FILENAME = "product_draft.json";
    private const string DRAFTS_FOLDER_NAME = "Drafts";
    
    private readonly string _localFolder;
    private readonly string _draftsFolder;

    public LocalDraftService()
    {
        // Sử dụng %LocalAppData%/MyShopClient cho unpackaged app
        _localFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MyShopClient"
        );
        _draftsFolder = Path.Combine(_localFolder, DRAFTS_FOLDER_NAME);
        
        // Đảm bảo các thư mục tồn tại
        EnsureDirectoriesExist();
    }

    private void EnsureDirectoriesExist()
    {
        try
        {
            if (!Directory.Exists(_localFolder))
            {
                Directory.CreateDirectory(_localFolder);
            }
            if (!Directory.Exists(_draftsFolder))
            {
                Directory.CreateDirectory(_draftsFolder);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating directories: {ex.Message}");
        }
    }

    #region Order Draft

    public async Task SaveDraftAsync(OrderDraft draft)
    {
        try
        {
            EnsureDirectoriesExist();
            var filePath = Path.Combine(_localFolder, DRAFT_FILENAME);
            var json = JsonSerializer.Serialize(draft, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving draft: {ex.Message}");
        }
    }

    public async Task<OrderDraft?> GetDraftAsync()
    {
        try
        {
            var filePath = Path.Combine(_localFolder, DRAFT_FILENAME);
            if (File.Exists(filePath))
            {
                var json = await File.ReadAllTextAsync(filePath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    return JsonSerializer.Deserialize<OrderDraft>(json);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error reading draft: {ex.Message}");
        }
        return null;
    }

    public Task ClearDraftAsync()
    {
        try
        {
            var filePath = Path.Combine(_localFolder, DRAFT_FILENAME);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting draft: {ex.Message}");
        }
        return Task.CompletedTask;
    }

    #endregion

    #region Product Draft

    public async Task SaveProductDraftAsync(ProductDraft draft)
    {
        try
        {
            EnsureDirectoriesExist();
            var filePath = Path.Combine(_localFolder, PRODUCT_DRAFT_FILENAME);
            var json = JsonSerializer.Serialize(draft, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving product draft: {ex.Message}");
        }
    }

    public async Task<ProductDraft?> GetProductDraftAsync()
    {
        try
        {
            var filePath = Path.Combine(_localFolder, PRODUCT_DRAFT_FILENAME);
            if (File.Exists(filePath))
            {
                var json = await File.ReadAllTextAsync(filePath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    return JsonSerializer.Deserialize<ProductDraft>(json);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error reading product draft: {ex.Message}");
        }
        return null;
    }

    public Task ClearProductDraftAsync()
    {
        try
        {
            // Delete JSON file
            var filePath = Path.Combine(_localFolder, PRODUCT_DRAFT_FILENAME);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // Delete Drafts folder contents
            if (Directory.Exists(_draftsFolder))
            {
                Directory.Delete(_draftsFolder, recursive: true);
                // Recreate empty folder
                Directory.CreateDirectory(_draftsFolder);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting product draft: {ex.Message}");
        }
        return Task.CompletedTask;
    }

    #endregion

    /// <summary>
    /// Lấy đường dẫn thư mục Drafts để lưu file tạm (images, etc.)
    /// </summary>
    public string GetDraftsFolder()
    {
        EnsureDirectoriesExist();
        return _draftsFolder;
    }
}

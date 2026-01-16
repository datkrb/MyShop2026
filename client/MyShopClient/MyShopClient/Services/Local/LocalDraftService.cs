using System;
using System.Threading.Tasks;
using Windows.Storage;
using System.Text.Json;
using MyShopClient.Models;
using System.IO;

namespace MyShopClient.Services.Local;

public interface ILocalDraftService
{
    Task SaveDraftAsync(OrderDraft draft);
    Task<OrderDraft?> GetDraftAsync();
    Task ClearDraftAsync();
}

public class LocalDraftService : ILocalDraftService
{
    private const string DRAFT_FILENAME = "order_draft.json";
    private readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;

    public async Task SaveDraftAsync(OrderDraft draft)
    {
        try
        {
            var json = JsonSerializer.Serialize(draft);
            var file = await _localFolder.CreateFileAsync(DRAFT_FILENAME, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, json);
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
            var item = await _localFolder.TryGetItemAsync(DRAFT_FILENAME);
            if (item is StorageFile file)
            {
                var json = await FileIO.ReadTextAsync(file);
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

    public async Task ClearDraftAsync()
    {
        try
        {
            var item = await _localFolder.TryGetItemAsync(DRAFT_FILENAME);
            if (item is StorageFile file)
            {
                await file.DeleteAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting draft: {ex.Message}");
        }
    }
}

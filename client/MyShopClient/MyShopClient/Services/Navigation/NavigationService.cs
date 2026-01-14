using System;
using Microsoft.UI.Xaml.Controls;

namespace MyShopClient.Services.Navigation;

public class NavigationService : INavigationService
{
    private const string LastVisitedPageKey = "LastVisitedPage";
    
    // We can keep specific navigation logic here if needed centrally, 
    // but for now we focus on the requested feature: Last Visited Page persistence.

    /// <summary>
    /// Gets the last visited page tag from local settings.
    /// </summary>
    /// <returns>The page tag, or "Dashboard" as default.</returns>
    public string GetLastVisitedPage()
    {
        try
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var value = localSettings.Values[LastVisitedPageKey];
            return value as string ?? "Dashboard";
        }
        catch
        {
            return "Dashboard";
        }
    }

    /// <summary>
    /// Saves the current page tag to local settings.
    /// </summary>
    /// <param name="pageTag">The page tag to save.</param>
    public void SaveLastVisitedPage(string pageTag)
    {
        try
        {
            if (string.IsNullOrEmpty(pageTag)) return;
            
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[LastVisitedPageKey] = pageTag;
        }
        catch
        {
            // Silently fail if settings cannot be saved
        }
    }
}

public interface INavigationService
{
    string GetLastVisitedPage();
    void SaveLastVisitedPage(string pageTag);
}

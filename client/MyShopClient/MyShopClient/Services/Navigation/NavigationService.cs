using System;
using Microsoft.UI.Xaml.Controls;

namespace MyShopClient.Services.Navigation;

public class NavigationService : INavigationService
{
    private const string LastVisitedPageKey = "LastVisitedPage";
    private Frame? _frame;

    /// <summary>
    /// Initialize the navigation service with the content frame.
    /// </summary>
    public void Initialize(Frame frame)
    {
        _frame = frame;
    }

    /// <summary>
    /// Gets the Frame used for navigation.
    /// </summary>
    public Frame? Frame => _frame ?? App.Current.ContentFrame;

    /// <summary>
    /// Check if we can navigate back.
    /// </summary>
    public bool CanGoBack => Frame?.CanGoBack ?? false;

    /// <summary>
    /// Navigate to a page type with optional parameter.
    /// </summary>
    public bool Navigate(Type pageType, object? parameter = null)
    {
        if (Frame == null) return false;
        return Frame.Navigate(pageType, parameter);
    }

    /// <summary>
    /// Navigate to a page type (generic version).
    /// </summary>
    public bool Navigate<TPage>(object? parameter = null) where TPage : Page
    {
        return Navigate(typeof(TPage), parameter);
    }

    /// <summary>
    /// Go back to the previous page.
    /// </summary>
    public void GoBack()
    {
        if (CanGoBack)
        {
            Frame?.GoBack();
        }
    }

    /// <summary>
    /// Gets the last visited page tag from local settings.
    /// </summary>
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
    Frame? Frame { get; }
    bool CanGoBack { get; }
    bool Navigate(Type pageType, object? parameter = null);
    bool Navigate<TPage>(object? parameter = null) where TPage : Page;
    void GoBack();
    string GetLastVisitedPage();
    void SaveLastVisitedPage(string pageTag);
}

using System;
using Microsoft.UI.Xaml.Controls;
using MyShopClient.Services.Config;

namespace MyShopClient.Services.Navigation;

public class NavigationService : INavigationService
{
    private const string LastVisitedPageKey = "LastVisitedPage";
    private Frame? _frame;
    private readonly AppSettingsService _appSettingsService;

    public NavigationService(AppSettingsService appSettingsService)
    {
        _appSettingsService = appSettingsService;
    }

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
    /// Gets the last visited page tag from settings.
    /// </summary>
    public string GetLastVisitedPage()
    {
        try
        {
            return _appSettingsService.GetLastVisitedPage();
        }
        catch
        {
            return "Dashboard";
        }
    }

    /// <summary>
    /// Saves the current page tag to settings.
    /// </summary>
    public void SaveLastVisitedPage(string pageTag)
    {
        try
        {
            if (string.IsNullOrEmpty(pageTag)) return;
            _appSettingsService.SaveLastVisitedPage(pageTag);
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
    void Initialize(Frame frame);
    bool Navigate(Type pageType, object? parameter = null);
    bool Navigate<TPage>(object? parameter = null) where TPage : Page;
    void GoBack();
    string GetLastVisitedPage();
    void SaveLastVisitedPage(string pageTag);
}

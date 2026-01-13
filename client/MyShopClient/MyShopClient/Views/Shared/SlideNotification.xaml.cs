using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using Windows.UI;

namespace MyShopClient.Views.Shared;

/// <summary>
/// Reusable slide-in notification component with auto-hide functionality
/// </summary>
public sealed partial class SlideNotification : UserControl
{
    private DispatcherTimer? _autoHideTimer;

    /// <summary>
    /// Duration in seconds before the notification auto-hides (default: 3 seconds)
    /// </summary>
    public double AutoHideDuration { get; set; } = 3;

    public SlideNotification()
    {
        this.InitializeComponent();
        
        // Setup auto-hide timer
        _autoHideTimer = new DispatcherTimer();
        _autoHideTimer.Tick += AutoHideTimer_Tick;
    }

    /// <summary>
    /// Show a notification with the specified message and severity
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="severity">Severity: Success, Warning, Error, or Info</param>
    /// <param name="title">Optional custom title (default: "Thông báo")</param>
    public void Show(string message, NotificationSeverity severity = NotificationSeverity.Info, string? title = null)
    {
        // Stop any existing timer
        _autoHideTimer?.Stop();
        
        // Update content
        MessageText.Text = message;
        TitleText.Text = title ?? "Thông báo";
        
        // Update icon and color based on severity
        UpdateAppearance(severity);
        
        // Make visible
        NotificationPanel.Visibility = Visibility.Visible;
        
        // Reset position to off-screen
        NotificationTranslate.X = 400;
        
        // Slide in from right animation
        var slideIn = new DoubleAnimation
        {
            From = 400,
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(350)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        
        var storyboard = new Storyboard();
        storyboard.Children.Add(slideIn);
        Storyboard.SetTarget(slideIn, NotificationTranslate);
        Storyboard.SetTargetProperty(slideIn, "X");
        storyboard.Begin();
        
        // Start auto-hide timer
        _autoHideTimer!.Interval = TimeSpan.FromSeconds(AutoHideDuration);
        _autoHideTimer.Start();
    }

    /// <summary>
    /// Show a success notification
    /// </summary>
    public void ShowSuccess(string message, string? title = null)
        => Show(message, NotificationSeverity.Success, title ?? "Thành công");

    /// <summary>
    /// Show a warning notification
    /// </summary>
    public void ShowWarning(string message, string? title = null)
        => Show(message, NotificationSeverity.Warning, title ?? "Cảnh báo");

    /// <summary>
    /// Show an error notification
    /// </summary>
    public void ShowError(string message, string? title = null)
        => Show(message, NotificationSeverity.Error, title ?? "Lỗi");

    /// <summary>
    /// Show an info notification
    /// </summary>
    public void ShowInfo(string message, string? title = null)
        => Show(message, NotificationSeverity.Info, title ?? "Thông báo");

    /// <summary>
    /// Manually hide the notification
    /// </summary>
    public void Hide()
    {
        _autoHideTimer?.Stop();
        HideWithAnimation();
    }

    private void AutoHideTimer_Tick(object? sender, object e)
    {
        _autoHideTimer?.Stop();
        HideWithAnimation();
    }

    private void HideWithAnimation()
    {
        // Slide out to right animation
        var slideOut = new DoubleAnimation
        {
            From = 0,
            To = 400,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        
        var storyboard = new Storyboard();
        storyboard.Children.Add(slideOut);
        Storyboard.SetTarget(slideOut, NotificationTranslate);
        Storyboard.SetTargetProperty(slideOut, "X");
        storyboard.Completed += (s, e) =>
        {
            NotificationPanel.Visibility = Visibility.Collapsed;
        };
        storyboard.Begin();
    }

    private void UpdateAppearance(NotificationSeverity severity)
    {
        switch (severity)
        {
            case NotificationSeverity.Success:
                NotificationIcon.Glyph = "\uE73E"; // Checkmark
                NotificationIcon.Foreground = new SolidColorBrush(Color.FromArgb(255, 16, 185, 129)); // Green
                NotificationPanel.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 16, 185, 129));
                break;
            case NotificationSeverity.Warning:
                NotificationIcon.Glyph = "\uE7BA"; // Warning
                NotificationIcon.Foreground = new SolidColorBrush(Color.FromArgb(255, 245, 158, 11)); // Amber
                NotificationPanel.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 245, 158, 11));
                break;
            case NotificationSeverity.Error:
                NotificationIcon.Glyph = "\uE711"; // X mark
                NotificationIcon.Foreground = new SolidColorBrush(Color.FromArgb(255, 239, 68, 68)); // Red
                NotificationPanel.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 239, 68, 68));
                break;
            default: // Info
                NotificationIcon.Glyph = "\uE946"; // Info
                NotificationIcon.Foreground = new SolidColorBrush(Color.FromArgb(255, 59, 130, 246)); // Blue
                NotificationPanel.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 59, 130, 246));
                break;
        }
    }
}

/// <summary>
/// Notification severity levels
/// </summary>
public enum NotificationSeverity
{
    Info,
    Success,
    Warning,
    Error
}

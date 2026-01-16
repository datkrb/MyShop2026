using System;
using Microsoft.UI.Xaml.Media;

namespace MyShopClient.Models;

public class Promotion
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // "PERCENTAGE" or "FIXED"
    public string DiscountType { get; set; } = "PERCENTAGE";
    
    public int DiscountValue { get; set; }
    
    public int? MinOrderValue { get; set; }
    public int? MaxDiscount { get; set; }
    
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }

    // Helper properties for display
    public string DiscountDisplay => DiscountType == "PERCENTAGE" 
        ? $"{DiscountValue}%" 
        : $"{DiscountValue:N0} đ";

    public string StatusDisplay 
    {
        get 
        {
            if (!IsActive) return "Inactive";
            if (DateTime.Now < StartDate) return "Pending";
            if (DateTime.Now > EndDate) return "Expired";
            return "Active";
        }
    }

    public SolidColorBrush StatusBackgroundColor
    {
        get
        {
            return StatusDisplay switch
            {
                "Active" => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 220, 252, 231)), // #DCFCE7
                "Pending" => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 254, 243, 199)), // #FEF3C7
                "Expired" => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 254, 226, 226)), // #FEE2E2
                _ => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 241, 245, 249)) // #F1F5F9
            };
        }
    }

    public SolidColorBrush StatusForegroundColor
    {
        get
        {
            return StatusDisplay switch
            {
                "Active" => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 34, 197, 94)), // #22C55E
                "Pending" => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 245, 158, 11)), // #F59E0B
                "Expired" => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 239, 68, 68)), // #EF4444
                _ => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 100, 116, 139)) // #64748B
            };
        }
    }

    public string UsageLimitDisplay => UsageLimit.HasValue ? UsageLimit.Value.ToString() : "∞";
}


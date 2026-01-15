namespace MyShopClient.ViewModels;

/// <summary>
/// Model for pagination button display
/// </summary>
public class PageButtonModel
{
    public int PageNumber { get; set; }
    public bool IsCurrentPage { get; set; }
    public bool IsEllipsis { get; set; }
    
    public string DisplayText => IsEllipsis ? "..." : PageNumber.ToString();
}

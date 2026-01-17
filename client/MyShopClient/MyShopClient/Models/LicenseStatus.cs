using System;

namespace MyShopClient.Models;

/// <summary>
/// Trạng thái license của cửa hàng
/// </summary>
public class LicenseStatus
{
    /// <summary>
    /// License đã được kích hoạt chưa
    /// </summary>
    public bool IsActivated { get; set; }

    /// <summary>
    /// Trial còn hiệu lực không
    /// </summary>
    public bool IsTrialValid { get; set; }

    /// <summary>
    /// Số ngày trial còn lại
    /// </summary>
    public int TrialDaysRemaining { get; set; }

    /// <summary>
    /// Ngày kích hoạt (nếu đã kích hoạt)
    /// </summary>
    public DateTime? ActivatedAt { get; set; }

    /// <summary>
    /// License có hợp lệ để sử dụng app không
    /// </summary>
    public bool IsValid => IsActivated || IsTrialValid;
}

/// <summary>
/// Kết quả kích hoạt license
/// </summary>
public class ActivationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

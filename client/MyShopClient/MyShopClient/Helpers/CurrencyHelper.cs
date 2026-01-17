using System.Globalization;

namespace MyShopClient.Helpers;

/// <summary>
/// Helper class for formatting currency values in Vietnamese Dong (VND) format.
/// Format: "100.000 đ" (dot as thousand separator, space before "đ")
/// </summary>
public static class CurrencyHelper
{
    private static readonly CultureInfo VietnameseCulture = new("vi-VN");

    /// <summary>
    /// Formats a decimal value as Vietnamese Dong currency.
    /// </summary>
    /// <param name="amount">The amount to format</param>
    /// <returns>Formatted string like "100.000 đ"</returns>
    public static string FormatVND(decimal amount)
    {
        // Use Vietnamese culture for proper thousand separator (dot)
        // N0 = Number format with 0 decimal places
        return $"{amount.ToString("N0", VietnameseCulture)} đ";
    }

    /// <summary>
    /// Formats a double value as Vietnamese Dong currency.
    /// </summary>
    /// <param name="amount">The amount to format</param>
    /// <returns>Formatted string like "100.000 đ"</returns>
    public static string FormatVND(double amount)
    {
        return FormatVND((decimal)amount);
    }

    /// <summary>
    /// Formats a decimal value as abbreviated Vietnamese Dong currency.
    /// Example: 1.500.000 -> "1,5tr đ", 500.000 -> "500k đ"
    /// </summary>
    /// <param name="amount">The amount to format</param>
    /// <returns>Abbreviated formatted string</returns>
    public static string FormatVNDShort(decimal amount)
    {
        if (amount >= 1_000_000_000) // Tỷ
        {
            return $"{amount / 1_000_000_000:0.#}tỷ đ";
        }
        if (amount >= 1_000_000) // Triệu
        {
            return $"{amount / 1_000_000:0.#}tr đ";
        }
        if (amount >= 1_000) // Nghìn
        {
            return $"{amount / 1_000:0.#}k đ";
        }
        return $"{amount:N0} đ";
    }

    /// <summary>
    /// Formats a double value as abbreviated Vietnamese Dong currency.
    /// </summary>
    public static string FormatVNDShort(double amount)
    {
        return FormatVNDShort((decimal)amount);
    }
}

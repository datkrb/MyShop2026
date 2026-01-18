using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Microsoft.UI.Xaml;

namespace MyShopClient.Services.Invoice;

public class InvoiceData
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public List<InvoiceItem> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class InvoiceItem
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class InvoiceService
{
    private const string ShopName = "MyShop";

    public InvoiceService()
    {
        // Set QuestPDF license to Community (free for small businesses)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<bool> GenerateAndSaveInvoiceAsync(InvoiceData data, Window window)
    {
        try
        {
            // Generate PDF bytes
            var pdfBytes = GeneratePdf(data);

            // Create file picker
            var picker = new FileSavePicker();
            
            // Get HWND for the picker
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            picker.SuggestedFileName = $"HoaDon_{data.InvoiceNumber}_{DateTime.Now:yyyyMMdd}";
            picker.FileTypeChoices.Add("PDF Document", new List<string> { ".pdf" });
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                await FileIO.WriteBytesAsync(file, pdfBytes);
                
                // Open the PDF file
                await Launcher.LaunchFileAsync(file);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error generating invoice: {ex.Message}");
            throw;
        }
    }

    private byte[] GeneratePdf(InvoiceData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                // Header
                page.Header().Element(c => ComposeHeader(c, data));

                // Content
                page.Content().Element(c => ComposeContent(c, data));

                // Footer
                page.Footer().Element(ComposeFooter);
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }

    private void ComposeHeader(IContainer container, InvoiceData data)
    {
        container.Column(column =>
        {
            // Shop Info Row
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(ShopName)
                        .FontSize(32)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);
                });

                row.ConstantItem(220).Column(col =>
                {
                    col.Item().AlignRight().Text("HÓA ĐƠN BÁN HÀNG")
                        .FontSize(18)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);
                    col.Item().AlignRight().Text($"Số: {data.InvoiceNumber}")
                        .FontSize(12)
                        .Bold();
                    col.Item().AlignRight().Text($"Ngày: {data.Date:dd/MM/yyyy HH:mm}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);
                });
            });

            column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

            // Customer Info
            column.Item().PaddingBottom(15).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("THÔNG TIN KHÁCH HÀNG").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingTop(5).Text($"Tên: {data.CustomerName}").FontSize(11);
                    if (!string.IsNullOrEmpty(data.CustomerPhone))
                        col.Item().Text($"SĐT: {data.CustomerPhone}").FontSize(11);
                    if (!string.IsNullOrEmpty(data.CustomerEmail))
                        col.Item().Text($"Email: {data.CustomerEmail}").FontSize(11);
                    if (!string.IsNullOrEmpty(data.CustomerAddress))
                        col.Item().Text($"Địa chỉ: {data.CustomerAddress}").FontSize(11);
                });

                row.ConstantItem(150).AlignRight().Column(col =>
                {
                    col.Item().Text("TRẠNG THÁI").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingTop(5).Text(GetVietnameseStatus(data.Status))
                        .FontSize(14)
                        .Bold()
                        .FontColor(GetStatusColor(data.Status));
                });
            });
        });
    }

    private void ComposeContent(IContainer container, InvoiceData data)
    {
        container.PaddingVertical(10).Column(column =>
        {
            // Table Header
            column.Item().Background(Colors.Blue.Darken2).Padding(8).Row(row =>
            {
                row.ConstantItem(40).Text("#").FontColor(Colors.White).Bold();
                row.RelativeItem(3).Text("Sản phẩm").FontColor(Colors.White).Bold();
                row.RelativeItem().AlignCenter().Text("SL").FontColor(Colors.White).Bold();
                row.RelativeItem().AlignRight().Text("Đơn giá").FontColor(Colors.White).Bold();
                row.RelativeItem().AlignRight().Text("Thành tiền").FontColor(Colors.White).Bold();
            });

            // Table Rows (alternating colors)
            for (int i = 0; i < data.Items.Count; i++)
            {
                var item = data.Items[i];
                var bgColor = i % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                column.Item().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Row(row =>
                {
                    row.ConstantItem(40).Text((i + 1).ToString());
                    row.RelativeItem(3).Text(item.ProductName);
                    row.RelativeItem().AlignCenter().Text(item.Quantity.ToString());
                    row.RelativeItem().AlignRight().Text(FormatCurrency(item.UnitPrice));
                    row.RelativeItem().AlignRight().Text(FormatCurrency(item.TotalPrice));
                });
            }

            // Totals
            column.Item().PaddingTop(15).Row(row =>
            {
                row.RelativeItem(); // Empty space

                row.ConstantItem(250).Column(col =>
                {
                    col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Row(r =>
                    {
                        r.RelativeItem().Text("Tạm tính:").FontSize(12);
                        r.RelativeItem().AlignRight().Text(FormatCurrency(data.Subtotal)).FontSize(12);
                    });

                    // Show discount row if there's a discount
                    if (data.DiscountAmount > 0)
                    {
                        col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Row(r =>
                        {
                            r.RelativeItem().Text("Giảm giá:").FontSize(12).FontColor(Colors.Green.Darken1);
                            r.RelativeItem().AlignRight().Text($"-{FormatCurrency(data.DiscountAmount)}").FontSize(12).FontColor(Colors.Green.Darken1);
                        });
                    }

                    col.Item().Background(Colors.Blue.Lighten5).Padding(8).Row(r =>
                    {
                        r.RelativeItem().Text("TỔNG CỘNG:").FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                        r.RelativeItem().AlignRight().Text(FormatCurrency(data.Total)).FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                    });
                });
            });

            // Thank you note
            column.Item().PaddingTop(30).AlignCenter().Text("Cảm ơn quý khách đã mua hàng!")
                .FontSize(12)
                .Italic()
                .FontColor(Colors.Grey.Darken1);
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("Trang ");
            text.CurrentPageNumber();
            text.Span(" / ");
            text.TotalPages();
        });
    }

    private string FormatCurrency(decimal amount)
    {
        return Helpers.CurrencyHelper.FormatVND(amount);
    }

    private string GetVietnameseStatus(string status)
    {
        return status switch
        {
            "PENDING" => "Chờ xử lý",
            "PAID" => "Đã thanh toán",
            "CANCELLED" => "Đã hủy",
            "DRAFT" => "Nháp",
            _ => status
        };
    }

    private string GetStatusColor(string status)
    {
        return status switch
        {
            "PENDING" => Colors.Orange.Darken1,
            "PAID" => Colors.Green.Darken1,
            "CANCELLED" => Colors.Red.Darken1,
            "DRAFT" => Colors.Grey.Darken1,
            _ => Colors.Grey.Darken1
        };
    }
}

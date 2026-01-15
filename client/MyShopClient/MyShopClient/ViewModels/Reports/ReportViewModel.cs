using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MyShopClient.Services.Api;
using LiveChartsCore.Kernel.Sketches;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShopClient.ViewModels;

public partial class ReportViewModel : ObservableObject
{
    private readonly IReportApiService _reportApiService;

    [ObservableProperty]
    private DateTimeOffset _startDate;

    [ObservableProperty]
    private DateTimeOffset _endDate;

    [ObservableProperty]
    private string _selectedReportType = "day";

    [ObservableProperty]
    private ISeries[] _revenueSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private ISeries[] _profitSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private ISeries[] _productSalesSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private ICartesianAxis[] _revenueXAxes = Array.Empty<ICartesianAxis>();

    [ObservableProperty]
    private string[] _reportTypes = { "day", "month", "year" };

    [ObservableProperty]
    private bool _isLoading;

    public ReportViewModel(IReportApiService reportApiService)
    {
        _reportApiService = reportApiService;
        var now = DateTime.Now;
        StartDate = new DateTimeOffset(new DateTime(now.Year, now.Month, 1));
        EndDate = new DateTimeOffset(new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month)));
        
        LoadDataCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            var start = StartDate.DateTime;
            var end = EndDate.DateTime;

            await Task.WhenAll(
                LoadRevenueAsync(start, end),
                LoadProfitAsync(start, end),
                LoadProductSalesAsync(start, end)
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadDataAsync Error: {ex.Message}");
            // Optional: Show error dialog or notification
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadRevenueAsync(DateTime start, DateTime end)
    {
        var data = await _reportApiService.GetRevenueReportAsync(start, end, SelectedReportType);
        
        var values = data.Select(x => (double)x.Revenue).ToArray();
        var labels = data.Select(x => x.Date).ToArray();

        RevenueSeries = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Name = "Revenue",
                Values = values
            }
        };

        RevenueXAxes = new ICartesianAxis[]
        {
            new Axis
            {
                Labels = labels,
                LabelsRotation = 0,
                SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)),
                SeparatorsAtCenter = false,
                TicksPaint = new SolidColorPaint(new SKColor(35, 35, 35)),
                TicksAtCenter = true
            }
        };
    }

    private async Task LoadProfitAsync(DateTime start, DateTime end)
    {
        var data = await _reportApiService.GetProfitReportAsync(start, end);

        ProfitSeries = new ISeries[]
        {
            new PieSeries<double> { Values = new double[] { (double)data.Profit }, Name = "Profit", InnerRadius = 50 },
            new PieSeries<double> { Values = new double[] { (double)data.Cost }, Name = "Cost", InnerRadius = 50 }
        };
    }

    [ObservableProperty]
    private ICartesianAxis[] _productSalesXAxes = Array.Empty<ICartesianAxis>();

    private async Task LoadProductSalesAsync(DateTime start, DateTime end)
    {
        var data = await _reportApiService.GetProductSalesReportAsync(start, end);

        // Top 5 products
        var topProducts = data.Take(5).ToList();


        ProductSalesSeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Name = "Quantity",
                Values = topProducts.Select(x => (double)x.Quantity).ToArray(),
                Fill = null,
                GeometrySize = 10,
                LineSmoothness = 0 // Straight lines for categorical data
            }
        };

        ProductSalesXAxes = new ICartesianAxis[]
        {
            new Axis
            {
                Labels = topProducts.Select(x => x.Product.Name).ToList(),
                LabelsRotation = 15
            }
        };
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using MyShopClient.Models;
using MyShopClient.ViewModels.Base;
using MyShopClient.Services.Api;
using SkiaSharp;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyShopClient.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly DashboardApiService _dashboardApiService;

    [ObservableProperty]
    private string _totalProducts = "0";

    [ObservableProperty]
    private string _dailyRevenue = "$0";

    [ObservableProperty]
    private string _dailyOrders = "0";

    [ObservableProperty]
    private string _pendingOrders = "0";

    // Percentage change properties
    [ObservableProperty]
    private string _productsChangeText = "+0%";

    [ObservableProperty]
    private bool _productsChangePositive = true;

    [ObservableProperty]
    private string _revenueChangeText = "+0%";

    [ObservableProperty]
    private bool _revenueChangePositive = true;

    [ObservableProperty]
    private string _ordersChangeText = "+0%";

    [ObservableProperty]
    private bool _ordersChangePositive = true;

    [ObservableProperty]
    private bool isLoading = true;

    // Line Chart (Revenue)
    [ObservableProperty]
    private ISeries[] revenueSeries = Array.Empty<ISeries>();
    
    [ObservableProperty]
    private IEnumerable<LiveChartsCore.Kernel.Sketches.ICartesianAxis> xAxes = Array.Empty<LiveChartsCore.Kernel.Sketches.ICartesianAxis>();
    
    public IEnumerable<LiveChartsCore.Kernel.Sketches.ICartesianAxis> YAxes { get; set; } = Array.Empty<LiveChartsCore.Kernel.Sketches.ICartesianAxis>();

    // Pie Chart (Donut)
    [ObservableProperty]
    private ISeries[] pieSeries = Array.Empty<ISeries>();

    public ObservableCollection<Product> TopSelling { get; } = new();
    public ObservableCollection<Order> RecentOrders { get; } = new();
    public ObservableCollection<Product> LowStock { get; } = new();

    public DashboardViewModel(DashboardApiService dashboardApiService)
    {
        _dashboardApiService = dashboardApiService;
        
        InitializeChartAxes();
        _ = LoadDashboardDataAsync();
    }

    private void InitializeChartAxes()
    {
        // X Axes will be set dynamically when data is loaded
        XAxes = Array.Empty<Axis>();

        // Y Axes Configuration
        YAxes = new Axis[]
        {
            new Axis
            {
                Labeler = value => Helpers.CurrencyHelper.FormatVNDShort((decimal)value),
                LabelsPaint = new SolidColorPaint(SKColors.Gray),
                SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200, 50)) { PathEffect = new DashEffect(new float[] { 5, 5 }) }
            }
        };
    }

    [RelayCommand]
    private async Task LoadDashboardDataAsync()
    {
        IsLoading = true;

        try
        {
            // Load all dashboard data in parallel
            var summaryTask = _dashboardApiService.GetSummaryAsync();
            var topSellingTask = _dashboardApiService.GetTopSellingAsync();
            var recentOrdersTask = _dashboardApiService.GetRecentOrdersAsync();
            var lowStockTask = _dashboardApiService.GetLowStockAsync();
            var revenueChartTask = _dashboardApiService.GetRevenueChartAsync();
            var categoryStatsTask = _dashboardApiService.GetCategoryStatsAsync();

            await Task.WhenAll(summaryTask, topSellingTask, recentOrdersTask, lowStockTask, revenueChartTask, categoryStatsTask);

            // Update Summary
            var summary = await summaryTask;
            if (summary != null)
            {
                TotalProducts = summary.TotalProducts.ToString("N0");
                DailyRevenue = Helpers.CurrencyHelper.FormatVND(summary.RevenueToday);
                DailyOrders = summary.TotalOrdersToday.ToString("N0");
                PendingOrders = summary.PendingOrders.ToString("N0");

                // Update percentage changes
                ProductsChangePositive = summary.ProductsChange >= 0;
                ProductsChangeText = $"{(ProductsChangePositive ? "+" : "")}{summary.ProductsChange:F1}%";

                RevenueChangePositive = summary.RevenueChange >= 0;
                RevenueChangeText = $"{(RevenueChangePositive ? "+" : "")}{summary.RevenueChange:F1}%";

                OrdersChangePositive = summary.OrdersChange >= 0;
                OrdersChangeText = $"{(OrdersChangePositive ? "+" : "")}{summary.OrdersChange:F1}%";
            }

            // Update Top Selling Products
            var topSelling = await topSellingTask;
            if (topSelling != null)
            {
                TopSelling.Clear();
                foreach (var product in topSelling)
                {
                    var imageUrl = product.Images?.FirstOrDefault()?.Url ?? "https://via.placeholder.com/150";
                    TopSelling.Add(new Product
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Category = product.Category?.Name ?? "Uncategorized",
                        Sold = product.TotalSold,
                        ImageUrl = imageUrl,
                        Stock = product.Stock,
                        Price = product.SalePrice
                    });
                }
            }

            // Update Pie Chart with Category Stats
            var categoryStats = await categoryStatsTask;
            if (categoryStats != null && categoryStats.Count > 0)
            {
                UpdatePieChart(categoryStats);
            }

            // Update Recent Orders
            var recentOrders = await recentOrdersTask;
            if (recentOrders != null)
            {
                RecentOrders.Clear();
                foreach (var order in recentOrders)
                {
                    // Use Order.FromApiOrder factory method
                    RecentOrders.Add(Order.FromApiOrder(order));
                }
            }

            // Update Low Stock Products
            var lowStock = await lowStockTask;
            if (lowStock != null)
            {
                LowStock.Clear();
                foreach (var product in lowStock)
                {
                    var imageUrl = product.Images?.FirstOrDefault()?.Url ?? "https://via.placeholder.com/150";
                    LowStock.Add(new Product
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Stock = product.Stock,
                        Category = product.Category?.Name ?? "",
                        ImageUrl = imageUrl
                    });
                }
            }

            // Update Revenue Chart
            var revenueChart = await revenueChartTask;
            if (revenueChart != null)
            {
                UpdateRevenueChart(revenueChart);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading dashboard: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateRevenueChart(Dictionary<string, decimal> dailyRevenue)
    {
        // Get day numbers from API data and sort them
        var days = dailyRevenue.Keys
            .Select(k => int.TryParse(k, out var d) ? d : 0)
            .Where(d => d > 0)
            .OrderBy(d => d)
            .ToList();

        if (days.Count == 0) return;

        // Get values in order
        var values = days.Select(day => (double)dailyRevenue[day.ToString()]).ToArray();

        // Update X-Axis labels dynamically based on received data
        XAxes = new Axis[]
        {
            new Axis
            {
                Labels = days.Select(d => d.ToString()).ToArray(),
                LabelsPaint = new SolidColorPaint(SKColors.Gray),
                SeparatorsPaint = null,
                MinStep = 1
            }
        };

        RevenueSeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = values,
                Fill = new LinearGradientPaint(
                    new[] { new SKColor(124, 92, 252, 128), new SKColor(124, 92, 252, 0) },
                    new SKPoint(0, 0),
                    new SKPoint(0, 1)),
                Stroke = new SolidColorPaint(new SKColor(124, 92, 252)) { StrokeThickness = 3 },
                GeometrySize = 8,
                GeometryStroke = new SolidColorPaint(new SKColor(124, 92, 252)) { StrokeThickness = 2 },
                GeometryFill = new SolidColorPaint(SKColors.White),
                LineSmoothness = 0.4
            }
        };
    }

    private void UpdatePieChart(List<CategoryStat> categories)
    {
        var colors = new[]
        {
            new SKColor(124, 92, 252),  // Primary
            new SKColor(236, 72, 153),  // Pink
            new SKColor(34, 197, 94),   // Green
            new SKColor(249, 115, 22),  // Orange
            new SKColor(59, 130, 246),  // Blue
            new SKColor(168, 85, 247),  // Purple
            new SKColor(20, 184, 166),  // Teal
            new SKColor(245, 158, 11)   // Amber
        };

        var series = new List<ISeries>();
        for (int i = 0; i < categories.Count; i++)
        {
            series.Add(new PieSeries<double>
            {
                Values = new double[] { categories[i].ProductCount },
                Name = categories[i].Name,
                InnerRadius = 60,
                Fill = new SolidColorPaint(colors[i % colors.Length])
            });
        }

        PieSeries = series.ToArray();
    }
}

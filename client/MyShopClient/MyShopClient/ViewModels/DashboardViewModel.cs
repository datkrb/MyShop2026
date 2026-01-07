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
    private string _totalInvoices = "0";

    [ObservableProperty]
    private bool isLoading = true;

    // Line Chart (Revenue)
    [ObservableProperty]
    private ISeries[] revenueSeries = Array.Empty<ISeries>();
    
    public IEnumerable<LiveChartsCore.Kernel.Sketches.ICartesianAxis> XAxes { get; set; } = Array.Empty<LiveChartsCore.Kernel.Sketches.ICartesianAxis>();
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
        // X Axes Configuration
        XAxes = new Axis[]
        {
            new Axis
            {
                Labels = new string[] { "1", "5", "10", "15", "20", "25", "30" },
                LabelsPaint = new SolidColorPaint(SKColors.Gray),
                SeparatorsPaint = null
            }
        };

        // Y Axes Configuration
        YAxes = new Axis[]
        {
            new Axis
            {
                Labeler = value => $"${value / 1000}k",
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

            await Task.WhenAll(summaryTask, topSellingTask, recentOrdersTask, lowStockTask, revenueChartTask);

            // Update Summary
            var summary = await summaryTask;
            if (summary != null)
            {
                TotalProducts = summary.TotalProducts.ToString("N0");
                DailyRevenue = $"${summary.RevenueToday:N0}";
                DailyOrders = summary.TotalOrdersToday.ToString("N0");
                // TODO: API does not have totalInvoices field - using mock data
                TotalInvoices = "1,135"; // Mock data
                System.Diagnostics.Debug.WriteLine($"Dashboard Summary: Products={TotalProducts}, Revenue={DailyRevenue}, Orders={DailyOrders}, Invoices={TotalInvoices}");
            }

            // Update Top Selling Products
            var topSelling = await topSellingTask;
            if (topSelling != null)
            {
                TopSelling.Clear();
                foreach (var product in topSelling)
                {
                    TopSelling.Add(new Product
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Category = product.Category?.Name ?? "Uncategorized",
                        Sold = product.TotalSold, // Calculated from OrderItems
                        ImageUrl = "https://via.placeholder.com/150",
                        Stock = product.Stock,
                        Price = product.SalePrice
                    });
                }

                // Update Pie Chart with Top Selling data
                UpdatePieChart(topSelling);
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
                    LowStock.Add(new Product
                    {
                        Name = product.Name,
                        Stock = product.Stock,
                        Category = product.Category?.Name ?? ""
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
            // Load mock data as fallback
            LoadMockData();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateRevenueChart(Dictionary<int, decimal> dailyRevenue)
    {
        // Sample every 5 days for display
        var sampledDays = new[] { 1, 5, 10, 15, 20, 25, 30 };
        var values = sampledDays.Select(day => (double)(dailyRevenue.ContainsKey(day) ? dailyRevenue[day] : 0)).ToArray();

        this.revenueSeries = new ISeries[]
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

    private void UpdatePieChart(List<ApiProduct> products)
    {
        var colors = new[]
        {
            new SKColor(124, 92, 252),  // Primary
            new SKColor(236, 72, 153),  // Pink
            new SKColor(34, 197, 94),   // Green
            new SKColor(249, 115, 22)   // Orange
        };

        var series = new List<ISeries>();
        for (int i = 0; i < Math.Min(products.Count, 4); i++)
        {
            series.Add(new PieSeries<double>
            {
                Values = new double[] { products[i].TotalSold },
                Name = products[i].Category?.Name ?? "Category",
                InnerRadius = 60,
                Fill = new SolidColorPaint(colors[i % colors.Length])
            });
        }

        this.pieSeries = series.ToArray();
    }

    private void LoadMockData()
    {
        // Fallback mock data if API fails
        TotalProducts = "1,456";
        DailyRevenue = "$3,345";
        DailyOrders = "126";
        TotalInvoices = "1,135";

        // Mock line chart
        this.revenueSeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = new double[] { 1200, 1900, 1500, 2800, 2200, 3100, 2900 },
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

        // Mock pie chart
        this.pieSeries = new ISeries[]
        {
            new PieSeries<double> { Values = new double[] { 342 }, Name = "Accessories", InnerRadius = 60, Fill = new SolidColorPaint(new SKColor(124, 92, 252)) },
            new PieSeries<double> { Values = new double[] { 215 }, Name = "Shoes", InnerRadius = 60, Fill = new SolidColorPaint(new SKColor(236, 72, 153)) },
            new PieSeries<double> { Values = new double[] { 189 }, Name = "Electronics", InnerRadius = 60, Fill = new SolidColorPaint(new SKColor(34, 197, 94)) },
            new PieSeries<double> { Values = new double[] { 145 }, Name = "Audio", InnerRadius = 60, Fill = new SolidColorPaint(new SKColor(249, 115, 22)) }
        };

        TopSelling.Add(new Product { Name = "Urban Backpack", Category = "Accessories", Sold = 342, ImageUrl = "https://via.placeholder.com/150" });
        TopSelling.Add(new Product { Name = "Nike Running", Category = "Shoes", Sold = 215, ImageUrl = "https://via.placeholder.com/150" });
        TopSelling.Add(new Product { Name = "iPhone 14 Case", Category = "Electronics", Sold = 189, ImageUrl = "https://via.placeholder.com/150" });

        RecentOrders.Add(new Order { OrderId = "#065499", CustomerName = "John Doe", CustomerAvatar = "https://ui-avatars.com/api/?name=John+Doe", ItemName = "Product", OrderDate = DateTime.Now, Status = "Paid", Price = 101 });
        
        LowStock.Add(new Product { Name = "White Cotton T-Shirt", Stock = 2 });
        LowStock.Add(new Product { Name = "Gaming Mousepad XL", Stock = 5 });
    }
}

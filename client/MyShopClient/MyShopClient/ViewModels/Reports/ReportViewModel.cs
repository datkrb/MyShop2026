using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MyShopClient.Models;
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
    private readonly ProductApiService _productApiService;

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

    // Category filter for Top 5 Products chart
    [ObservableProperty]
    private ObservableCollection<CategoryFilterItem> _categories = new();

    [ObservableProperty]
    private CategoryFilterItem? _selectedCategory;

    // Revenue Summary Properties
    [ObservableProperty]
    private string _totalRevenue = "0 đ";
    
    [ObservableProperty]
    private string _highestRevenueDay = "-";
    
    [ObservableProperty]
    private string _averageRevenue = "0 đ";
    
    [ObservableProperty]
    private string _revenueDataPoints = "0";

    // Top Products List
    [ObservableProperty]
    private ObservableCollection<TopProductItem> _topProductsList = new();

    // Profit Summary Properties
    [ObservableProperty]
    private string _profitRevenue = "0 đ";
    
    [ObservableProperty]
    private string _profitCost = "0 đ";
    
    [ObservableProperty]
    private string _netProfit = "0 đ";
    
    [ObservableProperty]
    private string _profitMargin = "0%";

    // KPI Section Properties
    [ObservableProperty]
    private ObservableCollection<KpiSalesItem> _kpiItems = new();

    [ObservableProperty]
    private KpiSalesItem? _myKpi;

    [ObservableProperty]
    private int _selectedKpiYear;

    [ObservableProperty]
    private int? _selectedKpiMonth;

    [ObservableProperty]
    private ObservableCollection<int> _kpiYears = new();

    [ObservableProperty]
    private ObservableCollection<int?> _kpiMonths = new();

    // NEW: Profit Time Series Chart Properties
    [ObservableProperty]
    private ISeries[] _profitTimeSeriesSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private ICartesianAxis[] _profitTimeSeriesXAxes = Array.Empty<ICartesianAxis>();

    // NEW: Single Product Analysis Properties
    [ObservableProperty]
    private ObservableCollection<ApiProduct> _availableProducts = new();

    [ObservableProperty]
    private ApiProduct? _selectedProduct;

    [ObservableProperty]
    private ISeries[] _singleProductSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private ICartesianAxis[] _singleProductXAxes = Array.Empty<ICartesianAxis>();

    // NEW: Category Sales Chart Properties
    [ObservableProperty]
    private ISeries[] _categorySalesSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private ICartesianAxis[] _categorySalesXAxes = Array.Empty<ICartesianAxis>();

    // NEW: Analysis Mode Toggle (Product vs Category)
    [ObservableProperty]
    private string _selectedAnalysisMode = "Product";

    public string[] AnalysisModes { get; } = { "Product", "Category" };

    [ObservableProperty]
    private CategoryFilterItem? _selectedCategoryForAnalysis;

    // NEW: Metric Type Filter (Quantity, Revenue, Profit)
    [ObservableProperty]
    private string _selectedMetricType = "Quantity";

    public string[] MetricTypes { get; } = { "Quantity", "Revenue", "Profit" };

    // Computed visibility properties
    public bool IsProductMode => SelectedAnalysisMode == "Product";
    public bool IsCategoryMode => SelectedAnalysisMode == "Category";

    public bool IsAdmin => App.Current.IsAdmin;

    // Computed properties for My KPI display (avoids null binding issues in XAML)
    public string MyKpiOrders => MyKpi?.Orders.ToString() ?? "0";
    public string MyKpiRevenue => MyKpi != null ? Helpers.CurrencyHelper.FormatVND(MyKpi.Revenue) : "0 đ";
    public string MyKpiRate => MyKpi != null ? $"{MyKpi.CommissionRate}%" : "0%";
    public string MyKpiCommission => MyKpi != null ? Helpers.CurrencyHelper.FormatVND(MyKpi.Commission) : "0 đ";


    public ReportViewModel(IReportApiService reportApiService, ProductApiService productApiService)
    {
        _reportApiService = reportApiService;
        _productApiService = productApiService;
        var now = DateTime.Now;
        StartDate = new DateTimeOffset(new DateTime(now.Year, now.Month, 1));
        EndDate = new DateTimeOffset(new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month)));
        
        // Initialize KPI year/month filters
        InitializeKpiFilters();
        
        _ = LoadCategoriesAsync();
        LoadDataCommand.Execute(null);
    }

    private void InitializeKpiFilters()
    {
        var currentYear = DateTime.Now.Year;
        for (int y = currentYear; y >= currentYear - 5; y--)
        {
            KpiYears.Add(y);
        }
        SelectedKpiYear = currentYear;

        // Month options: null = All, 1-12
        KpiMonths.Add(null); // "All Months"
        for (int m = 1; m <= 12; m++)
        {
            KpiMonths.Add(m);
        }
        SelectedKpiMonth = null;
    }

    private async Task LoadCategoriesAsync()
    {
        var cats = await _productApiService.GetCategoriesAsync();
        Categories.Clear();
        Categories.Add(new CategoryFilterItem { Id = null, Name = "All Categories" });
        if (cats != null)
        {
            foreach (var cat in cats)
            {
                Categories.Add(new CategoryFilterItem { Id = cat.Id, Name = cat.Name });
            }
        }
        SelectedCategory = Categories.FirstOrDefault();

        // Load available products for single product chart
        await LoadAvailableProductsAsync();
    }

    private async Task LoadAvailableProductsAsync()
    {
        var products = await _productApiService.GetProductsAsync(1, 100, null, null);
        AvailableProducts.Clear();
        if (products.Data != null)
        {
            foreach (var product in products.Data)
            {
                AvailableProducts.Add(product);
            }
        }
        SelectedProduct = AvailableProducts.FirstOrDefault();
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
                LoadProductSalesAsync(start, end),
                LoadProfitTimeSeriesAsync(start, end),
                LoadCategorySalesAsync(start, end),
                LoadKpiAsync()
            );

            // Load single product separately if selected
            if (SelectedProduct != null)
            {
                await LoadSingleProductSalesAsync(start, end);
            }
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
        var categoryId = SelectedCategory?.Id;
        var data = await _reportApiService.GetRevenueReportAsync(start, end, SelectedReportType, categoryId);
        
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

        // Update Revenue Summary
        var totalRev = data.Sum(x => x.Revenue);
        TotalRevenue = Helpers.CurrencyHelper.FormatVND(totalRev);
        RevenueDataPoints = data.Count.ToString();
        
        if (data.Count > 0)
        {
            var avgRev = totalRev / data.Count;
            AverageRevenue = Helpers.CurrencyHelper.FormatVND(avgRev);
            
            var maxItem = data.OrderByDescending(x => x.Revenue).First();
            HighestRevenueDay = $"{maxItem.Date}: {Helpers.CurrencyHelper.FormatVND(maxItem.Revenue)}";
        }
        else
        {
            AverageRevenue = "0 đ";
            HighestRevenueDay = "-";
        }
    }

    private async Task LoadProfitAsync(DateTime start, DateTime end)
    {
        var categoryId = SelectedCategory?.Id;
        var data = await _reportApiService.GetProfitReportAsync(start, end, categoryId);

        ProfitSeries = new ISeries[]
        {
            new PieSeries<double> { Values = new double[] { (double)data.Profit }, Name = "Profit", InnerRadius = 50 },
            new PieSeries<double> { Values = new double[] { (double)data.Cost }, Name = "Cost", InnerRadius = 50 }
        };

        // Update Profit Summary
        ProfitRevenue = Helpers.CurrencyHelper.FormatVND(data.Revenue);
        ProfitCost = Helpers.CurrencyHelper.FormatVND(data.Cost);
        NetProfit = Helpers.CurrencyHelper.FormatVND(data.Profit);
        ProfitMargin = $"{data.ProfitMargin:N1}%";
    }

    [ObservableProperty]
    private ICartesianAxis[] _productSalesXAxes = Array.Empty<ICartesianAxis>();

    private async Task LoadProductSalesAsync(DateTime start, DateTime end)
    {
        var categoryId = SelectedCategory?.Id;
        var data = await _reportApiService.GetTopProductsTimeSeriesAsync(start, end, categoryId);

        if (data.Products.Count == 0 || data.Dates.Count == 0)
        {
            ProductSalesSeries = Array.Empty<ISeries>();
            ProductSalesXAxes = Array.Empty<ICartesianAxis>();
            return;
        }

        // Define colors for each product line
        var colors = new SKColor[]
        {
            new SKColor(33, 150, 243),   // Blue
            new SKColor(76, 175, 80),    // Green
            new SKColor(255, 152, 0),    // Orange
            new SKColor(156, 39, 176),   // Purple
            new SKColor(244, 67, 54)     // Red
        };

        // Create a LineSeries for each product
        var seriesList = new List<ISeries>();
        for (int i = 0; i < data.Products.Count; i++)
        {
            var product = data.Products[i];
            var seriesData = data.Series.FirstOrDefault(s => s.ProductId == product.Id);
            if (seriesData == null) continue;

            var color = colors[i % colors.Length];
            seriesList.Add(new LineSeries<double>
            {
                Name = product.Name,
                Values = seriesData.Data.Select(x => (double)x).ToArray(),
                Stroke = new SolidColorPaint(color) { StrokeThickness = 2 },
                Fill = null,
                GeometrySize = 8,
                GeometryStroke = new SolidColorPaint(color) { StrokeThickness = 2 },
                GeometryFill = new SolidColorPaint(SKColors.White),
                LineSmoothness = 0.3
            });
        }

        ProductSalesSeries = seriesList.ToArray();

        // Format dates for X-axis (show only day/month for readability)
        var formattedDates = data.Dates.Select(d => 
        {
            if (DateTime.TryParse(d, out var date))
                return date.ToString("dd/MM");
            return d;
        }).ToList();

        ProductSalesXAxes = new ICartesianAxis[]
        {
            new Axis
            {
                Labels = formattedDates,
                LabelsRotation = 45,
                TextSize = 10
            }
        };

        // Update Top Products List
        TopProductsList.Clear();
        for (int i = 0; i < data.Products.Count; i++)
        {
            var product = data.Products[i];
            var seriesData = data.Series.FirstOrDefault(s => s.ProductId == product.Id);
            var totalQty = seriesData?.Data.Sum() ?? 0;
            TopProductsList.Add(new TopProductItem { Name = product.Name, Quantity = totalQty });
        }
    }

    [RelayCommand]
    private async Task LoadKpiAsync()
    {
        try
        {
            if (IsAdmin)
            {
                // Admin can see all employees' KPI
                var data = await _reportApiService.GetKpiSalesReportAsync(SelectedKpiYear, SelectedKpiMonth);
                KpiItems.Clear();
                foreach (var item in data)
                {
                    KpiItems.Add(item);
                }
                MyKpi = null;
            }
            else
            {
                // Sale user can only see their own KPI
                MyKpi = await _reportApiService.GetMyKpiAsync(SelectedKpiYear, SelectedKpiMonth);
                KpiItems.Clear();
                
                // Notify computed properties changed
                OnPropertyChanged(nameof(MyKpiOrders));
                OnPropertyChanged(nameof(MyKpiRevenue));
                OnPropertyChanged(nameof(MyKpiRate));
                OnPropertyChanged(nameof(MyKpiCommission));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadKpiAsync Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Load profit time series chart
    /// </summary>
    private async Task LoadProfitTimeSeriesAsync(DateTime start, DateTime end)
    {
        try
        {
            var categoryId = SelectedCategory?.Id;
            var data = await _reportApiService.GetProfitTimeSeriesAsync(start, end, SelectedReportType, categoryId);

            if (data.Count == 0)
            {
                ProfitTimeSeriesSeries = Array.Empty<ISeries>();
                ProfitTimeSeriesXAxes = Array.Empty<ICartesianAxis>();
                return;
            }

            var profitValues = data.Select(x => (double)x.Profit).ToArray();
            var revenueValues = data.Select(x => (double)x.Revenue).ToArray();
            var labels = data.Select(x => x.Date).ToArray();

            ProfitTimeSeriesSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Name = "Profit",
                    Values = profitValues,
                    Stroke = new SolidColorPaint(new SKColor(34, 197, 94)) { StrokeThickness = 2 },
                    Fill = null,
                    GeometrySize = 6,
                    GeometryFill = new SolidColorPaint(new SKColor(34, 197, 94)),
                    LineSmoothness = 0.3
                },
                new LineSeries<double>
                {
                    Name = "Revenue",
                    Values = revenueValues,
                    Stroke = new SolidColorPaint(new SKColor(59, 130, 246)) { StrokeThickness = 2 },
                    Fill = null,
                    GeometrySize = 6,
                    GeometryFill = new SolidColorPaint(new SKColor(59, 130, 246)),
                    LineSmoothness = 0.3
                }
            };

            ProfitTimeSeriesXAxes = new ICartesianAxis[]
            {
                new Axis { Labels = labels, LabelsRotation = 45, TextSize = 10 }
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadProfitTimeSeriesAsync Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Load single product sales chart
    /// </summary>
    private async Task LoadSingleProductSalesAsync(DateTime start, DateTime end)
    {
        try
        {
            if (SelectedProduct == null)
            {
                SingleProductSeries = Array.Empty<ISeries>();
                SingleProductXAxes = Array.Empty<ICartesianAxis>();
                return;
            }

            var data = await _reportApiService.GetProductSalesByIdAsync(SelectedProduct.Id, start, end, SelectedReportType);

            // Generate all dates in range based on SelectedReportType
            var allDates = GenerateDateRange(start, end, SelectedReportType);
            var dataDict = data.ToDictionary(x => x.Date, x => x);

            var values = new List<double>();
            var labels = new List<string>();

            foreach (var date in allDates)
            {
                labels.Add(date);
                if (dataDict.TryGetValue(date, out var item))
                {
                    // Select value based on SelectedMetricType
                    var value = SelectedMetricType switch
                    {
                        "Revenue" => (double)item.Revenue,
                        "Profit" => (double)item.Revenue * 0.3, // Estimate profit as 30% of revenue (actual profit would need cost data)
                        _ => (double)item.Quantity
                    };
                    values.Add(value);
                }
                else
                {
                    values.Add(0);
                }
            }

            SingleProductSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Name = SelectedMetricType,
                    Values = values.ToArray(),
                    Stroke = new SolidColorPaint(new SKColor(124, 92, 252)) { StrokeThickness = 2 },
                    Fill = null,
                    GeometrySize = 6,
                    GeometryFill = new SolidColorPaint(new SKColor(124, 92, 252)),
                    LineSmoothness = 0.3
                }
            };

            // Format labels for display
            var formattedLabels = labels.Select(d =>
            {
                if (DateTime.TryParse(d, out var date))
                    return date.ToString("dd/MM");
                return d;
            }).ToList();

            SingleProductXAxes = new ICartesianAxis[]
            {
                new Axis { Labels = formattedLabels, LabelsRotation = 45, TextSize = 10 }
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadSingleProductSalesAsync Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Generate all dates in range based on report type
    /// </summary>
    private List<string> GenerateDateRange(DateTime start, DateTime end, string reportType)
    {
        var dates = new List<string>();
        var current = start;

        while (current <= end)
        {
            if (reportType == "day")
            {
                dates.Add(current.ToString("yyyy-MM-dd"));
                current = current.AddDays(1);
            }
            else if (reportType == "month")
            {
                dates.Add($"{current.Year}-{current.Month:D2}");
                current = current.AddMonths(1);
            }
            else // year
            {
                dates.Add($"{current.Year}");
                current = current.AddYears(1);
            }
        }

        return dates;
    }

    /// <summary>
    /// Load category sales comparison chart
    /// </summary>
    private async Task LoadCategorySalesAsync(DateTime start, DateTime end)
    {
        try
        {
            var data = await _reportApiService.GetCategorySalesTimeSeriesAsync(start, end, SelectedReportType);

            if (data.Dates.Count == 0 || data.Series.Count == 0)
            {
                CategorySalesSeries = Array.Empty<ISeries>();
                CategorySalesXAxes = Array.Empty<ICartesianAxis>();
                return;
            }

            var colors = new SKColor[]
            {
                new SKColor(33, 150, 243),   // Blue
                new SKColor(76, 175, 80),    // Green
                new SKColor(255, 152, 0),    // Orange
                new SKColor(156, 39, 176),   // Purple
                new SKColor(244, 67, 54),    // Red
                new SKColor(0, 188, 212),    // Cyan
                new SKColor(255, 235, 59),   // Yellow
                new SKColor(121, 85, 72)     // Brown
            };

            // Filter series based on selected category
            var filteredSeries = data.Series;
            if (SelectedCategoryForAnalysis != null && SelectedCategoryForAnalysis.Id.HasValue)
            {
                // Filter to only the selected category
                filteredSeries = data.Series.Where(s => s.CategoryId == SelectedCategoryForAnalysis.Id.Value).ToList();
            }

            var seriesList = new List<ISeries>();
            for (int i = 0; i < filteredSeries.Count; i++)
            {
                var seriesItem = filteredSeries[i];
                var color = colors[i % colors.Length];

                seriesList.Add(new LineSeries<double>
                {
                    Name = seriesItem.CategoryName,
                    Values = seriesItem.Data.Select(x => (double)x).ToArray(),
                    Stroke = new SolidColorPaint(color) { StrokeThickness = 2 },
                    Fill = null,
                    GeometrySize = 6,
                    GeometryFill = new SolidColorPaint(color),
                    LineSmoothness = 0.3
                });
            }

            CategorySalesSeries = seriesList.ToArray();

            var formattedDates = data.Dates.Select(d =>
            {
                if (DateTime.TryParse(d, out var date))
                    return date.ToString("dd/MM");
                return d;
            }).ToList();

            CategorySalesXAxes = new ICartesianAxis[]
            {
                new Axis { Labels = formattedDates, LabelsRotation = 45, TextSize = 10 }
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadCategorySalesAsync Error: {ex.Message}");
        }
    }

    partial void OnSelectedProductChanged(ApiProduct? value)
    {
        if (value != null && !IsLoading && IsProductMode)
        {
            _ = LoadSingleProductSalesAsync(StartDate.DateTime, EndDate.DateTime);
        }
    }

    partial void OnSelectedAnalysisModeChanged(string value)
    {
        OnPropertyChanged(nameof(IsProductMode));
        OnPropertyChanged(nameof(IsCategoryMode));

        if (!IsLoading)
        {
            if (value == "Product" && SelectedProduct != null)
            {
                _ = LoadSingleProductSalesAsync(StartDate.DateTime, EndDate.DateTime);
            }
            else if (value == "Category")
            {
                _ = LoadCategorySalesAsync(StartDate.DateTime, EndDate.DateTime);
            }
        }
    }

    partial void OnSelectedCategoryForAnalysisChanged(CategoryFilterItem? value)
    {
        if (value != null && !IsLoading && IsCategoryMode)
        {
            _ = LoadCategorySalesAsync(StartDate.DateTime, EndDate.DateTime);
        }
    }

    partial void OnSelectedMetricTypeChanged(string value)
    {
        if (!IsLoading && IsProductMode && SelectedProduct != null)
        {
            _ = LoadSingleProductSalesAsync(StartDate.DateTime, EndDate.DateTime);
        }
    }
}

// Simple model for category filter dropdown
public class CategoryFilterItem
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public override string ToString() => Name;
}

// Model for Top Products list
public class TopProductItem
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishCash.Models;
using FishCash.Services;
using FishCash.Views;

namespace FishCash.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly ITradingService _tradingService;

    // ═══ Stats Cards ═══
    [ObservableProperty] private decimal totalSales;
    [ObservableProperty] private decimal totalPurchase;
    [ObservableProperty] private decimal totalProfit;
    [ObservableProperty] private int totalSessions;
    [ObservableProperty] private string currentDateText;

    // ═══ Bar Chart ═══
    [ObservableProperty] private BarChartDrawable chartDrawable;
    [ObservableProperty] private string selectedPeriod = "7 ngày";
    [ObservableProperty] private string chartTooltipText = string.Empty;
    [ObservableProperty] private bool isChartTooltipVisible;
    public string[] PeriodOptions => new[] { "7 ngày", "Tháng", "Năm" };

    // ═══ Recent Trade Orders ═══
    public ObservableCollection<TradeOrder> RecentTradeOrders { get; } = new();

    // ═══ Product Stats ═══
    public ObservableCollection<ProductStat> ProductStats { get; } = new();

    // ═══ Product Detail Modal ═══
    [ObservableProperty] private bool isProductDetailVisible;
    [ObservableProperty] private ProductStat? selectedProductStat;
    [ObservableProperty] private BarChartDrawable productChartDrawable;
    [ObservableProperty] private string selectedProductPeriod = "Tháng";
    public string[] ProductPeriodOptions => new[] { "Tháng", "Năm" };

    public DashboardViewModel(ITradingService tradingService)
    {
        _tradingService = tradingService;
        Title = "Tổng quan";
        CurrentDateText = DateTime.Now.ToString("dddd, dd/MM/yyyy");
        ChartDrawable = new BarChartDrawable();
        ProductChartDrawable = new BarChartDrawable();
    }

    [RelayCommand]
    public async Task LoadDashboardDataAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;

            TotalSales = await _tradingService.GetTotalSalesAsync();
            TotalPurchase = await _tradingService.GetTotalPurchaseAsync();
            TotalProfit = await _tradingService.GetTotalProfitAsync();
            TotalSessions = await _tradingService.GetTotalSessionsAsync();

            await LoadChartDataAsync();

            var recentOrders = await _tradingService.GetRecentTradeOrdersAsync(8);
            RecentTradeOrders.Clear();
            foreach (var o in recentOrders) RecentTradeOrders.Add(o);

            var prodStats = await _tradingService.GetProductStatsAsync();
            ProductStats.Clear();
            foreach (var p in prodStats) ProductStats.Add(p);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể tải dữ liệu: {ex.Message}", "OK");
        }
        finally { IsBusy = false; }
    }

    // ═══ Chart Period Switching ═══

    [RelayCommand]
    public async Task SelectPeriodAsync(string period)
    {
        SelectedPeriod = period;
        await LoadChartDataAsync();
    }

    private async Task LoadChartDataAsync()
    {
        List<DailyStat> data;
        switch (SelectedPeriod)
        {
            case "Tháng": data = await _tradingService.GetMonthlyStatsAsync(12); break;
            case "Năm": data = await _tradingService.GetYearlyStatsAsync(5); break;
            default: data = await _tradingService.GetDailyStatsAsync(7); break;
        }
        ChartDrawable = new BarChartDrawable { Data = data };
    }

    // ═══ Chart Interaction (tooltip on hover/tap) ═══

    [RelayCommand]
    public void ChartInteraction(PointF point)
    {
        if (ChartDrawable?.HitAreas == null) return;
        foreach (var area in ChartDrawable.HitAreas)
        {
            if (area.Rect.Contains(point))
            {
                ChartDrawable.HoveredIndex = area.Index;
                ChartDrawable.HoveredIsPurchase = area.IsPurchase;
                var value = area.IsPurchase ? area.Stat.TotalPurchase : area.Stat.TotalSales;
                var label = area.IsPurchase ? "Mua" : "Bán";
                ChartTooltipText = $"{area.Stat.Label} — {label}: {value:N0} đ";
                IsChartTooltipVisible = true;
                OnPropertyChanged(nameof(ChartDrawable));
                return;
            }
        }
        // No bar hit
        ChartDrawable.HoveredIndex = null;
        IsChartTooltipVisible = false;
        OnPropertyChanged(nameof(ChartDrawable));
    }

    // ═══ Product Detail Modal ═══

    [RelayCommand]
    public async Task ShowProductDetailAsync(ProductStat stat)
    {
        if (stat == null) return;
        SelectedProductStat = stat;
        SelectedProductPeriod = "Tháng";
        await LoadProductChartAsync();
        IsProductDetailVisible = true;
    }

    [RelayCommand] public void CloseProductDetail() { IsProductDetailVisible = false; SelectedProductStat = null; }

    [RelayCommand]
    public async Task SelectProductPeriodAsync(string period)
    {
        SelectedProductPeriod = period;
        await LoadProductChartAsync();
    }

    private async Task LoadProductChartAsync()
    {
        if (SelectedProductStat == null) return;
        List<DailyStat> data;
        if (SelectedProductPeriod == "Năm")
            data = await _tradingService.GetProductYearlyStatsAsync(SelectedProductStat.ProductName);
        else
            data = await _tradingService.GetProductMonthlyStatsAsync(SelectedProductStat.ProductName);
        ProductChartDrawable = new BarChartDrawable { Data = data };
    }

    // ═══ Navigation ═══

    [RelayCommand]
    public async Task NavigateToAsync(string routeName)
    {
        if (string.IsNullOrWhiteSpace(routeName)) return;
        if (!routeName.StartsWith("//")) routeName = $"//{routeName}";
        await Shell.Current.GoToAsync(routeName);
    }
}

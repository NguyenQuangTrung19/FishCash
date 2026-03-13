using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishCash.Models;
using FishCash.Services;
using FishCash.Views;

namespace FishCash.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly IOrderService _orderService;

    [ObservableProperty]
    private decimal totalRevenue;

    [ObservableProperty]
    private int totalOrders;

    [ObservableProperty]
    private int totalProductsSold;

    [ObservableProperty]
    private string currentDateText;

    public ObservableCollection<Order> RecentOrders { get; } = new();

    public DashboardViewModel(IOrderService orderService)
    {
        _orderService = orderService;
        Title = "Tổng quan";
        CurrentDateText = DateTime.Now.ToString("dddd, dd/MM/yyyy");
    }

    [RelayCommand]
    public async Task LoadDashboardDataAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            TotalRevenue = await _orderService.GetTotalRevenueAsync();
            TotalOrders = await _orderService.GetTotalOrdersAsync();
            TotalProductsSold = await _orderService.GetTotalProductsSoldAsync();

            var recentOrders = await _orderService.GetRecentOrdersAsync(5);
            RecentOrders.Clear();
            foreach (var order in recentOrders)
            {
                RecentOrders.Add(order);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể tải dữ liệu thống kê: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task NavigateToAsync(string routeName)
    {
        if (string.IsNullOrWhiteSpace(routeName)) return;

        // Routing in Shell requires '//' for flyout items to reset the navigation stack securely
        if (!routeName.StartsWith("//"))
        {
            routeName = $"//{routeName}";
        }

        await Shell.Current.GoToAsync(routeName);
    }
}

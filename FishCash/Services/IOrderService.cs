using FishCash.Models;

namespace FishCash.Services;

/// <summary>
/// Interface for order management operations
/// </summary>
public interface IOrderService
{
    Task<Order> CreateOrderAsync(List<OrderDetail> orderDetails, PaymentMethod paymentMethod);
    
    // Thống kê Dashboard
    Task<decimal> GetTotalRevenueAsync();
    Task<int> GetTotalOrdersAsync();
    Task<int> GetTotalProductsSoldAsync();
    Task<List<Order>> GetRecentOrdersAsync(int count);
}

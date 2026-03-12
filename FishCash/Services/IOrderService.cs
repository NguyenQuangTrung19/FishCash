using FishCash.Models;

namespace FishCash.Services;

/// <summary>
/// Interface for order management operations
/// </summary>
public interface IOrderService
{
    Task<Order> CreateOrderAsync(List<OrderDetail> details, PaymentMethod paymentMethod);
    Task<List<Order>> GetRecentOrdersAsync();
}

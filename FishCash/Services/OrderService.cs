using FishCash.Data;
using FishCash.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FishCash.Services;

/// <summary>
/// Service for creating and managing customer orders
/// </summary>
public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrderService> _logger;

    public OrderService(AppDbContext context, ILogger<OrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Order> CreateOrderAsync(List<OrderDetail> details, PaymentMethod paymentMethod)
    {
        try
        {
            var totalAmount = details.Sum(d => d.SubTotal);
            
            var order = new Order
            {
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                PaymentMethod = paymentMethod,
                Status = OrderStatus.Completed
            };

            // Create associated transaction record
            var transaction = new Transaction
            {
                TransactionType = TransactionType.Income,
                Amount = totalAmount,
                TransactionDate = order.OrderDate,
                Note = $"Thanh toán đơn hàng - {paymentMethod}"
            };

            order.Transaction = transaction;

            foreach (var detail in details)
            {
                order.OrderDetails.Add(detail);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Order created: #{Id}, Total: {Amount}, Method: {Method}", 
                order.Id, totalAmount, paymentMethod);

            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order with {Count} items", details.Count);
            throw;
        }
    }
    public async Task<decimal> GetTotalRevenueAsync()
    {
        try
        {
            return await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed)
                .SumAsync(o => o.TotalAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total revenue");
            return 0;
        }
    }

    public async Task<int> GetTotalOrdersAsync()
    {
        try
        {
            return await _context.Orders.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total orders count");
            return 0;
        }
    }

    public async Task<int> GetTotalProductsSoldAsync()
    {
        try
        {
            return (int)await _context.OrderDetails.SumAsync(d => d.Quantity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total products sold");
            return 0;
        }
    }

    public async Task<List<Order>> GetRecentOrdersAsync(int count = 50)
    {
        try
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading recent orders");
            throw;
        }
    }
}

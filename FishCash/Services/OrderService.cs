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
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IDbContextFactory<AppDbContext> contextFactory, ILogger<OrderService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<Order> CreateOrderAsync(List<OrderDetail> details, PaymentMethod paymentMethod)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
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

            context.Orders.Add(order);
            await context.SaveChangesAsync();

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
            using var context = _contextFactory.CreateDbContext();
            return await context.Orders
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
            using var context = _contextFactory.CreateDbContext();
            return await context.Orders.CountAsync();
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
            using var context = _contextFactory.CreateDbContext();
            return (int)await context.OrderDetails.SumAsync(d => d.Quantity);
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
            using var context = _contextFactory.CreateDbContext();
            return await context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
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

using FishCash.Data;
using FishCash.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FishCash.Services;

/// <summary>
/// Service for managing trading sessions - the core broker logic.
/// </summary>
public class TradingService : ITradingService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<TradingService> _logger;

    public TradingService(IDbContextFactory<AppDbContext> contextFactory, ILogger<TradingService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    // ═══ Sessions ═══

    public async Task<List<TradingSession>> GetSessionsAsync(int count = 20)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.TradingSessions
                .Include(s => s.TradeOrders).ThenInclude(o => o.Partner)
                .Include(s => s.TradeOrders).ThenInclude(o => o.Details).ThenInclude(d => d.Product)
                .OrderByDescending(s => s.SessionDate)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex) { _logger.LogError(ex, "Error loading sessions"); throw; }
    }

    public async Task<TradingSession?> GetSessionByIdAsync(int id)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.TradingSessions
                .Include(s => s.TradeOrders).ThenInclude(o => o.Partner)
                .Include(s => s.TradeOrders).ThenInclude(o => o.Details).ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error loading session {Id}", id); throw; }
    }

    public async Task<TradingSession> CreateSessionAsync(string? note = null)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var session = new TradingSession { SessionDate = DateTime.Now, Note = note ?? string.Empty };
            context.TradingSessions.Add(session);
            await context.SaveChangesAsync();
            _logger.LogInformation("Session created: #{Id}", session.Id);
            return session;
        }
        catch (Exception ex) { _logger.LogError(ex, "Error creating session"); throw; }
    }

    public async Task SaveSessionAsync(int sessionId, string? note = null)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var session = await context.TradingSessions.FindAsync(sessionId)
                ?? throw new InvalidOperationException("Phiên không tồn tại");
            if (note != null) session.Note = note;
            await context.SaveChangesAsync();
        }
        catch (Exception ex) { _logger.LogError(ex, "Error saving session {Id}", sessionId); throw; }
    }

    // ═══ Trade Orders ═══

    public async Task<TradeOrder> AddTradeOrderAsync(int sessionId, int? partnerId, string partnerName,
        TradeOrderType orderType, List<TradeOrderDetail> details, string? note = null)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var session = await context.TradingSessions.FindAsync(sessionId)
                ?? throw new InvalidOperationException("Phiên không tồn tại");

            var totalAmount = details.Sum(d => d.SubTotal);
            var order = new TradeOrder
            {
                TradingSessionId = sessionId, PartnerId = partnerId, PartnerName = partnerName,
                OrderType = orderType, OrderDate = DateTime.Now, TotalAmount = totalAmount,
                Note = note ?? string.Empty
            };
            foreach (var d in details) order.Details.Add(d);
            context.TradeOrders.Add(order);

            if (orderType == TradeOrderType.Purchase) session.TotalPurchase += totalAmount;
            else session.TotalSales += totalAmount;

            await context.SaveChangesAsync();
            if (partnerId.HasValue) await context.Entry(order).Reference(o => o.Partner).LoadAsync();
            _logger.LogInformation("Order added: {Type} #{Id}, {Amount}", orderType, order.Id, totalAmount);
            return order;
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException?.Message ?? "No inner";
            _logger.LogError(ex, "Error adding order. Inner: {Inner}", inner);
            throw new InvalidOperationException($"{ex.Message} | Inner: {inner}", ex);
        }
    }

    public async Task RemoveTradeOrderAsync(int tradeOrderId)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var order = await context.TradeOrders.Include(o => o.Details)
                .FirstOrDefaultAsync(o => o.Id == tradeOrderId);
            if (order == null) return;

            var session = await context.TradingSessions.FindAsync(order.TradingSessionId);
            if (session != null)
            {
                if (order.OrderType == TradeOrderType.Purchase) session.TotalPurchase -= order.TotalAmount;
                else session.TotalSales -= order.TotalAmount;
                context.TradeOrderDetails.RemoveRange(order.Details);
                context.TradeOrders.Remove(order);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "Error removing order {Id}", tradeOrderId); throw; }
    }

    // ═══ Edit Order Details ═══

    public async Task<TradeOrderDetail> AddDetailToOrderAsync(int orderId, TradeOrderDetail detail)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var order = await context.TradeOrders.FindAsync(orderId)
                ?? throw new InvalidOperationException("Đơn hàng không tồn tại");
            var session = await context.TradingSessions.FindAsync(order.TradingSessionId)
                ?? throw new InvalidOperationException("Phiên không tồn tại");

            detail.TradeOrderId = orderId;
            context.TradeOrderDetails.Add(detail);

            order.TotalAmount += detail.SubTotal;
            if (order.OrderType == TradeOrderType.Purchase) session.TotalPurchase += detail.SubTotal;
            else session.TotalSales += detail.SubTotal;

            await context.SaveChangesAsync();

            // Load product nav
            await context.Entry(detail).Reference(d => d.Product).LoadAsync();
            _logger.LogInformation("Detail added to order #{OrderId}: {Product} x{Qty}",
                orderId, detail.Product?.Name, detail.Quantity);
            return detail;
        }
        catch (Exception ex) { _logger.LogError(ex, "Error adding detail to order {Id}", orderId); throw; }
    }

    public async Task RemoveDetailFromOrderAsync(int detailId)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var detail = await context.TradeOrderDetails.FindAsync(detailId);
            if (detail == null) return;

            var order = await context.TradeOrders.FindAsync(detail.TradeOrderId);
            if (order == null) return;
            var session = await context.TradingSessions.FindAsync(order.TradingSessionId);
            if (session == null) return;

            order.TotalAmount -= detail.SubTotal;
            if (order.OrderType == TradeOrderType.Purchase) session.TotalPurchase -= detail.SubTotal;
            else session.TotalSales -= detail.SubTotal;

            context.TradeOrderDetails.Remove(detail);
            await context.SaveChangesAsync();
            _logger.LogInformation("Detail #{DetailId} removed from order #{OrderId}", detailId, order.Id);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error removing detail {Id}", detailId); throw; }
    }

    public async Task UpdateTradeOrderAsync(int orderId, List<TradeOrderDetail> newDetails)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var order = await context.TradeOrders.Include(o => o.Details)
                .FirstOrDefaultAsync(o => o.Id == orderId)
                ?? throw new InvalidOperationException("Đơn hàng không tồn tại");
            var session = await context.TradingSessions.FindAsync(order.TradingSessionId)
                ?? throw new InvalidOperationException("Phiên không tồn tại");

            // Remove old amount from session
            if (order.OrderType == TradeOrderType.Purchase) session.TotalPurchase -= order.TotalAmount;
            else session.TotalSales -= order.TotalAmount;

            // Remove old details
            context.TradeOrderDetails.RemoveRange(order.Details);

            // Add new details
            var newTotal = newDetails.Sum(d => d.SubTotal);
            foreach (var d in newDetails)
            {
                d.TradeOrderId = orderId;
                d.Id = 0; // ensure new
                context.TradeOrderDetails.Add(d);
            }

            // Update order total
            order.TotalAmount = newTotal;
            if (order.OrderType == TradeOrderType.Purchase) session.TotalPurchase += newTotal;
            else session.TotalSales += newTotal;

            await context.SaveChangesAsync();
            _logger.LogInformation("Order #{Id} updated: {Count} details, total {Total}", orderId, newDetails.Count, newTotal);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error updating order {Id}", orderId); throw; }
    }

    // ═══ Dashboard Stats ═══

    public async Task<decimal> GetTotalProfitAsync()
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var sessions = await context.TradingSessions.ToListAsync();
            return sessions.Sum(s => s.TotalSales - s.TotalPurchase);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting profit"); return 0; }
    }

    public async Task<int> GetTotalSessionsAsync()
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.TradingSessions.CountAsync();
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting session count"); return 0; }
    }

    public async Task<decimal> GetTotalPurchaseAsync()
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.TradingSessions.SumAsync(s => s.TotalPurchase);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting total purchase"); return 0; }
    }

    public async Task<decimal> GetTotalSalesAsync()
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.TradingSessions.SumAsync(s => s.TotalSales);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting total sales"); return 0; }
    }

    public async Task<List<DailyStat>> GetDailyStatsAsync(int days = 7)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var startDate = DateTime.Now.Date.AddDays(-(days - 1));
            var orders = await context.TradeOrders.Where(o => o.OrderDate >= startDate).ToListAsync();
            var result = new List<DailyStat>();
            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var dayOrders = orders.Where(o => o.OrderDate.Date == date);
                result.Add(new DailyStat
                {
                    Date = date, Label = date.ToString("dd/MM"),
                    TotalPurchase = dayOrders.Where(o => o.OrderType == TradeOrderType.Purchase).Sum(o => o.TotalAmount),
                    TotalSales = dayOrders.Where(o => o.OrderType == TradeOrderType.Sale).Sum(o => o.TotalAmount)
                });
            }
            return result;
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting daily stats"); return new(); }
    }

    public async Task<List<DailyStat>> GetMonthlyStatsAsync(int months = 12)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-(months - 1));
            var orders = await context.TradeOrders.Where(o => o.OrderDate >= startDate).ToListAsync();
            var result = new List<DailyStat>();
            for (int i = 0; i < months; i++)
            {
                var date = startDate.AddMonths(i);
                var monthOrders = orders.Where(o => o.OrderDate.Year == date.Year && o.OrderDate.Month == date.Month);
                result.Add(new DailyStat
                {
                    Date = date, Label = $"T{date.Month}/{date.Year % 100}",
                    TotalPurchase = monthOrders.Where(o => o.OrderType == TradeOrderType.Purchase).Sum(o => o.TotalAmount),
                    TotalSales = monthOrders.Where(o => o.OrderType == TradeOrderType.Sale).Sum(o => o.TotalAmount)
                });
            }
            return result;
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting monthly stats"); return new(); }
    }

    public async Task<List<DailyStat>> GetYearlyStatsAsync(int years = 5)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var startYear = DateTime.Now.Year - (years - 1);
            var orders = await context.TradeOrders.Where(o => o.OrderDate.Year >= startYear).ToListAsync();
            var result = new List<DailyStat>();
            for (int i = 0; i < years; i++)
            {
                var year = startYear + i;
                var yearOrders = orders.Where(o => o.OrderDate.Year == year);
                result.Add(new DailyStat
                {
                    Date = new DateTime(year, 1, 1), Label = year.ToString(),
                    TotalPurchase = yearOrders.Where(o => o.OrderType == TradeOrderType.Purchase).Sum(o => o.TotalAmount),
                    TotalSales = yearOrders.Where(o => o.OrderType == TradeOrderType.Sale).Sum(o => o.TotalAmount)
                });
            }
            return result;
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting yearly stats"); return new(); }
    }

    public async Task<List<TradeOrder>> GetRecentTradeOrdersAsync(int count = 10)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.TradeOrders
                .Include(o => o.Partner)
                .Include(o => o.Details).ThenInclude(d => d.Product)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting recent orders"); return new(); }
    }

    public async Task<List<ProductStat>> GetProductStatsAsync()
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var details = await context.TradeOrderDetails
                .Include(d => d.Product)
                .Include(d => d.TradeOrder)
                .ToListAsync();

            return details
                .GroupBy(d => new { Name = d.Product?.Name ?? "?", Unit = d.Unit ?? d.Product?.Unit ?? "kg" })
                .Select(g => new ProductStat
                {
                    ProductName = g.Key.Name,
                    Unit = g.Key.Unit,
                    TotalPurchaseQty = g.Where(d => d.TradeOrder?.OrderType == TradeOrderType.Purchase).Sum(d => d.Quantity),
                    TotalSalesQty = g.Where(d => d.TradeOrder?.OrderType == TradeOrderType.Sale).Sum(d => d.Quantity),
                    TotalPurchaseAmount = g.Where(d => d.TradeOrder?.OrderType == TradeOrderType.Purchase).Sum(d => d.SubTotal),
                    TotalSalesAmount = g.Where(d => d.TradeOrder?.OrderType == TradeOrderType.Sale).Sum(d => d.SubTotal),
                })
                .OrderByDescending(p => p.TotalPurchaseAmount + p.TotalSalesAmount)
                .ToList();
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting product stats"); return new(); }
    }

    public async Task<List<DailyStat>> GetProductMonthlyStatsAsync(string productName)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-11);
            var details = await context.TradeOrderDetails
                .Include(d => d.Product).Include(d => d.TradeOrder)
                .Where(d => d.Product != null && d.Product.Name == productName && d.TradeOrder != null && d.TradeOrder.OrderDate >= startDate)
                .ToListAsync();
            var result = new List<DailyStat>();
            for (int i = 0; i < 12; i++)
            {
                var date = startDate.AddMonths(i);
                var monthDetails = details.Where(d => d.TradeOrder!.OrderDate.Year == date.Year && d.TradeOrder.OrderDate.Month == date.Month);
                result.Add(new DailyStat
                {
                    Date = date, Label = $"T{date.Month}",
                    TotalPurchase = monthDetails.Where(d => d.TradeOrder!.OrderType == TradeOrderType.Purchase).Sum(d => d.SubTotal),
                    TotalSales = monthDetails.Where(d => d.TradeOrder!.OrderType == TradeOrderType.Sale).Sum(d => d.SubTotal)
                });
            }
            return result;
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting product monthly stats"); return new(); }
    }

    public async Task<List<DailyStat>> GetProductYearlyStatsAsync(string productName)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var startYear = DateTime.Now.Year - 4;
            var details = await context.TradeOrderDetails
                .Include(d => d.Product).Include(d => d.TradeOrder)
                .Where(d => d.Product != null && d.Product.Name == productName && d.TradeOrder != null && d.TradeOrder.OrderDate.Year >= startYear)
                .ToListAsync();
            var result = new List<DailyStat>();
            for (int i = 0; i < 5; i++)
            {
                var year = startYear + i;
                var yearDetails = details.Where(d => d.TradeOrder!.OrderDate.Year == year);
                result.Add(new DailyStat
                {
                    Date = new DateTime(year, 1, 1), Label = year.ToString(),
                    TotalPurchase = yearDetails.Where(d => d.TradeOrder!.OrderType == TradeOrderType.Purchase).Sum(d => d.SubTotal),
                    TotalSales = yearDetails.Where(d => d.TradeOrder!.OrderType == TradeOrderType.Sale).Sum(d => d.SubTotal)
                });
            }
            return result;
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting product yearly stats"); return new(); }
    }
}

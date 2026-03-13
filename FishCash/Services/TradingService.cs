using FishCash.Data;
using FishCash.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FishCash.Services;

/// <summary>
/// Service for managing trading sessions - the core broker logic.
/// Handles creating sessions, adding purchase/sale orders, and calculating profit.
/// </summary>
public class TradingService : ITradingService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TradingService> _logger;

    public TradingService(AppDbContext context, ILogger<TradingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<TradingSession>> GetSessionsAsync(int count = 20)
    {
        try
        {
            return await _context.TradingSessions
                .Include(s => s.TradeOrders)
                    .ThenInclude(o => o.Partner)
                .Include(s => s.TradeOrders)
                    .ThenInclude(o => o.Details)
                        .ThenInclude(d => d.Product)
                .OrderByDescending(s => s.SessionDate)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading trading sessions");
            throw;
        }
    }

    public async Task<TradingSession?> GetSessionByIdAsync(int id)
    {
        try
        {
            return await _context.TradingSessions
                .Include(s => s.TradeOrders)
                    .ThenInclude(o => o.Partner)
                .Include(s => s.TradeOrders)
                    .ThenInclude(o => o.Details)
                        .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading session {Id}", id);
            throw;
        }
    }

    public async Task<TradingSession> CreateSessionAsync(string? note = null)
    {
        try
        {
            var session = new TradingSession
            {
                SessionDate = DateTime.Now,
                Note = note ?? string.Empty,
                Status = SessionStatus.Active
            };

            _context.TradingSessions.Add(session);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Trading session created: #{Id}", session.Id);
            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating trading session");
            throw;
        }
    }

    public async Task<TradeOrder> AddTradeOrderAsync(int sessionId, int partnerId,
        TradeOrderType orderType, List<TradeOrderDetail> details, string? note = null)
    {
        try
        {
            var session = await _context.TradingSessions.FindAsync(sessionId)
                ?? throw new InvalidOperationException("Phiên giao dịch không tồn tại");

            if (session.Status == SessionStatus.Completed)
                throw new InvalidOperationException("Phiên giao dịch đã hoàn tất, không thể thêm đơn");

            var totalAmount = details.Sum(d => d.SubTotal);

            var order = new TradeOrder
            {
                TradingSessionId = sessionId,
                PartnerId = partnerId,
                OrderType = orderType,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                Note = note ?? string.Empty
            };

            foreach (var detail in details)
            {
                order.Details.Add(detail);
            }

            _context.TradeOrders.Add(order);

            // Update session totals
            if (orderType == TradeOrderType.Purchase)
                session.TotalPurchase += totalAmount;
            else
                session.TotalSales += totalAmount;

            await _context.SaveChangesAsync();

            // Reload with navigation properties
            await _context.Entry(order).Reference(o => o.Partner).LoadAsync();

            _logger.LogInformation("Trade order added: {Type} #{Id}, Amount: {Amount}, Partner: {PartnerId}",
                orderType, order.Id, totalAmount, partnerId);

            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding trade order to session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task RemoveTradeOrderAsync(int tradeOrderId)
    {
        try
        {
            var order = await _context.TradeOrders
                .Include(o => o.Details)
                .FirstOrDefaultAsync(o => o.Id == tradeOrderId);

            if (order == null) return;

            var session = await _context.TradingSessions.FindAsync(order.TradingSessionId);
            if (session != null && session.Status == SessionStatus.Active)
            {
                // Update session totals
                if (order.OrderType == TradeOrderType.Purchase)
                    session.TotalPurchase -= order.TotalAmount;
                else
                    session.TotalSales -= order.TotalAmount;

                _context.TradeOrderDetails.RemoveRange(order.Details);
                _context.TradeOrders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing trade order {Id}", tradeOrderId);
            throw;
        }
    }

    public async Task CompleteSessionAsync(int sessionId)
    {
        try
        {
            var session = await _context.TradingSessions.FindAsync(sessionId)
                ?? throw new InvalidOperationException("Phiên giao dịch không tồn tại");

            session.Status = SessionStatus.Completed;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Session #{Id} completed. Purchase: {P}, Sales: {S}, Profit: {Pr}",
                session.Id, session.TotalPurchase, session.TotalSales, session.Profit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing session {Id}", sessionId);
            throw;
        }
    }

    public async Task<decimal> GetTotalProfitAsync()
    {
        try
        {
            var sessions = await _context.TradingSessions
                .Where(s => s.Status == SessionStatus.Completed)
                .ToListAsync();
            return sessions.Sum(s => s.TotalSales - s.TotalPurchase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total profit");
            return 0;
        }
    }

    public async Task<int> GetTotalSessionsAsync()
    {
        try
        {
            return await _context.TradingSessions.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sessions count");
            return 0;
        }
    }
}

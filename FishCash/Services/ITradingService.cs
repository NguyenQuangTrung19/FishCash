using FishCash.Models;

namespace FishCash.Services;

/// <summary>
/// Interface for trading session management (broker buy/sell operations)
/// </summary>
public interface ITradingService
{
    // Trading Sessions
    Task<List<TradingSession>> GetSessionsAsync(int count = 20);
    Task<TradingSession?> GetSessionByIdAsync(int id);
    Task<TradingSession> CreateSessionAsync(string? note = null);
    Task SaveSessionAsync(int sessionId, string? note = null);

    // Trade Orders within a session
    Task<TradeOrder> AddTradeOrderAsync(int sessionId, int? partnerId, string partnerName,
        TradeOrderType orderType, List<TradeOrderDetail> details, string? note = null);
    Task RemoveTradeOrderAsync(int tradeOrderId);

    // Edit existing order (add/remove details)
    Task<TradeOrderDetail> AddDetailToOrderAsync(int orderId, TradeOrderDetail detail);
    Task RemoveDetailFromOrderAsync(int detailId);
    Task UpdateTradeOrderAsync(int orderId, List<TradeOrderDetail> newDetails);

    // Dashboard stats
    Task<decimal> GetTotalProfitAsync();
    Task<int> GetTotalSessionsAsync();
    Task<decimal> GetTotalPurchaseAsync();
    Task<decimal> GetTotalSalesAsync();
    Task<List<DailyStat>> GetDailyStatsAsync(int days = 7);
    Task<List<DailyStat>> GetMonthlyStatsAsync(int months = 12);
    Task<List<DailyStat>> GetYearlyStatsAsync(int years = 5);
    Task<List<TradeOrder>> GetRecentTradeOrdersAsync(int count = 10);
    Task<List<ProductStat>> GetProductStatsAsync();
    Task<List<DailyStat>> GetProductMonthlyStatsAsync(string productName);
    Task<List<DailyStat>> GetProductYearlyStatsAsync(string productName);
}

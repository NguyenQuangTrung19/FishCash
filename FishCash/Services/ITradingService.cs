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
    Task CompleteSessionAsync(int sessionId);

    // Trade Orders within a session
    Task<TradeOrder> AddTradeOrderAsync(int sessionId, int partnerId, TradeOrderType orderType,
        List<TradeOrderDetail> details, string? note = null);
    Task RemoveTradeOrderAsync(int tradeOrderId);

    // Dashboard stats
    Task<decimal> GetTotalProfitAsync();
    Task<int> GetTotalSessionsAsync();
}

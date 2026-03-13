using System.ComponentModel.DataAnnotations;

namespace FishCash.Models;

/// <summary>
/// Represents a purchase or sale order within a trading session.
/// Each order is linked to one partner (supplier or buyer).
/// </summary>
public class TradeOrder
{
    public int Id { get; set; }

    public int TradingSessionId { get; set; }
    public TradingSession? TradingSession { get; set; }

    public int PartnerId { get; set; }
    public Partner? Partner { get; set; }

    public TradeOrderType OrderType { get; set; } = TradeOrderType.Purchase;

    public DateTime OrderDate { get; set; } = DateTime.Now;

    [Range(0, double.MaxValue)]
    public decimal TotalAmount { get; set; }

    [MaxLength(500)]
    public string Note { get; set; } = string.Empty;

    // Navigation
    public ICollection<TradeOrderDetail> Details { get; set; } = new List<TradeOrderDetail>();
}

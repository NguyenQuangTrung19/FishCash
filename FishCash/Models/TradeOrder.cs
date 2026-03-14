using System.ComponentModel.DataAnnotations;

namespace FishCash.Models;

/// <summary>
/// Represents a purchase or sale order within a trading session.
/// Each order can be linked to an existing partner OR have a typed-in partner name.
/// </summary>
public class TradeOrder
{
    public int Id { get; set; }

    public int TradingSessionId { get; set; }
    public TradingSession? TradingSession { get; set; }

    // Nullable: if user types partner name directly, PartnerId is null
    public int? PartnerId { get; set; }
    public Partner? Partner { get; set; }

    // Direct partner name input (used when PartnerId is null, or as display cache)
    [MaxLength(200)]
    public string PartnerName { get; set; } = string.Empty;

    /// <summary>
    /// Display name: prioritize linked Partner, fallback to typed PartnerName
    /// </summary>
    public string DisplayPartnerName => Partner?.Name ?? PartnerName;

    public TradeOrderType OrderType { get; set; } = TradeOrderType.Purchase;

    public DateTime OrderDate { get; set; } = DateTime.Now;

    [Range(0, double.MaxValue)]
    public decimal TotalAmount { get; set; }

    [MaxLength(500)]
    public string Note { get; set; } = string.Empty;

    // Navigation
    public ICollection<TradeOrderDetail> Details { get; set; } = new List<TradeOrderDetail>();
}

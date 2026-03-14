using System.ComponentModel.DataAnnotations;

namespace FishCash.Models;

/// <summary>
/// Represents a trading session that groups purchase orders and sale orders together.
/// Calculates total purchase, total sales, and profit.
/// </summary>
public class TradingSession
{
    public int Id { get; set; }

    public DateTime SessionDate { get; set; } = DateTime.Now;

    [MaxLength(500)]
    public string Note { get; set; } = string.Empty;

    /// <summary>Tổng tiền mua vào</summary>
    public decimal TotalPurchase { get; set; }

    /// <summary>Tổng tiền bán ra</summary>
    public decimal TotalSales { get; set; }

    /// <summary>Lợi nhuận = TotalSales - TotalPurchase</summary>
    public decimal Profit => TotalSales - TotalPurchase;


    // Navigation
    public ICollection<TradeOrder> TradeOrders { get; set; } = new List<TradeOrder>();
}

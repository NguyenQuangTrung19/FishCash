using System.ComponentModel.DataAnnotations;

namespace FishCash.Models;

/// <summary>
/// Line item within a trade order (product, quantity, price)
/// </summary>
public class TradeOrderDetail
{
    public int Id { get; set; }

    public int TradeOrderId { get; set; }
    public TradeOrder? TradeOrder { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Range(0.001, double.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
    public decimal Quantity { get; set; }

    [MaxLength(20)]
    public string Unit { get; set; } = "kg";

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal SubTotal { get; set; }
}

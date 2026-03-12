using System.ComponentModel.DataAnnotations;

namespace FishCash.Models;

/// <summary>
/// Represents a line item within an order (product, quantity, price)
/// </summary>
public class OrderDetail
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    
    [Range(0.001, double.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
    public decimal Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal SubTotal { get; set; }
}

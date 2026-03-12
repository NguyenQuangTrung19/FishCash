using System.ComponentModel.DataAnnotations;

namespace FishCash.Models;

/// <summary>
/// Represents a customer order with payment info and associated details
/// </summary>
public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public decimal TotalAmount { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public OrderStatus Status { get; set; } = OrderStatus.Completed;

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public Transaction? Transaction { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace FishCash.Models;

/// <summary>
/// Represents a financial transaction (income from orders or manual expenses)
/// </summary>
public class Transaction
{
    public int Id { get; set; }

    public int? OrderId { get; set; } // Null if manual income/expense (not tied to an order)
    public Order? Order { get; set; }
    
    public TransactionType TransactionType { get; set; } = TransactionType.Income;

    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.Now;

    [MaxLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
    public string Note { get; set; } = string.Empty;
}

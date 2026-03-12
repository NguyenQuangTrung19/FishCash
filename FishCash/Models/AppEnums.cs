namespace FishCash.Models;

/// <summary>
/// Payment method for an order
/// </summary>
public enum PaymentMethod
{
    Cash,
    QrTransfer
}

/// <summary>
/// Status of an order
/// </summary>
public enum OrderStatus
{
    Completed,
    Cancelled
}

/// <summary>
/// Type of financial transaction
/// </summary>
public enum TransactionType
{
    Income,
    Expense
}

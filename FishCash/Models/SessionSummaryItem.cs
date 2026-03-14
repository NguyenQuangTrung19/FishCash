namespace FishCash.Models;

/// <summary>
/// Summary of a single product within a trading session (all in kg + VND)
/// </summary>
public class SessionSummaryItem
{
    public string ProductName { get; set; } = string.Empty;

    // Quantities (all in kg)
    public decimal PurchaseQtyKg { get; set; }
    public decimal SalesQtyKg { get; set; }
    public decimal RemainingQtyKg => PurchaseQtyKg - SalesQtyKg;

    // Amounts (VND)
    public decimal PurchaseAmount { get; set; }
    public decimal SalesAmount { get; set; }
    public decimal Profit => SalesAmount - PurchaseAmount;
}

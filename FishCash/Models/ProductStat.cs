namespace FishCash.Models;

/// <summary>
/// Product quantity statistics (total bought/sold per product)
/// </summary>
public class ProductStat
{
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = "kg";
    public decimal TotalPurchaseQty { get; set; }
    public decimal TotalSalesQty { get; set; }
    public decimal TotalPurchaseAmount { get; set; }
    public decimal TotalSalesAmount { get; set; }
}

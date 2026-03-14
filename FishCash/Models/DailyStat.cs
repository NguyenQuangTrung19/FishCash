namespace FishCash.Models;

/// <summary>
/// Period-based trading statistics for dashboard chart.
/// Used for daily, monthly, and yearly views.
/// </summary>
public class DailyStat
{
    public DateTime Date { get; set; }
    public string Label { get; set; } = string.Empty; // "15/03", "T3/2026", "2026"
    public decimal TotalPurchase { get; set; }
    public decimal TotalSales { get; set; }
}

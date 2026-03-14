using FishCash.Models;
using Microsoft.Maui.Graphics;

namespace FishCash.Views;

/// <summary>
/// Custom bar chart with tooltip support.
/// Tracks bar hit areas so ViewModel can detect hover/tap and show tooltip.
/// </summary>
public class BarChartDrawable : IDrawable
{
    public List<DailyStat> Data { get; set; } = new();

    // Tooltip state (set by ViewModel via interaction)
    public int? HoveredIndex { get; set; }
    public bool HoveredIsPurchase { get; set; }

    // Bar hit areas (populated after draw, used for hit testing)
    public List<BarHitArea> HitAreas { get; } = new();

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        HitAreas.Clear();
        if (Data == null || Data.Count == 0) return;

        var width = dirtyRect.Width;
        var height = dirtyRect.Height;
        var padding = 48f;
        var bottomPadding = 50f;
        var chartWidth = width - padding * 2;
        var chartHeight = height - padding - bottomPadding;

        var maxValue = Data.Max(d => Math.Max(d.TotalPurchase, d.TotalSales));
        if (maxValue == 0) maxValue = 1;

        var barGroupWidth = chartWidth / Data.Count;
        var barWidth = Math.Min(barGroupWidth * 0.3f, 40f);
        var gap = barGroupWidth * 0.05f;

        // Grid lines
        canvas.StrokeColor = Color.FromArgb("#e2e8f0");
        canvas.StrokeSize = 1;
        for (int i = 0; i <= 4; i++)
        {
            var y = padding + (chartHeight / 4f * i);
            canvas.DrawLine(padding, y, width - padding, y);
        }

        // Y-axis labels
        canvas.FontSize = 9;
        canvas.FontColor = Color.FromArgb("#94a3b8");
        for (int i = 0; i <= 4; i++)
        {
            var val = maxValue * (4 - i) / 4;
            var y = padding + (chartHeight / 4f * i);
            canvas.DrawString(FormatAmount(val), 0, y - 6, padding - 4, 12, HorizontalAlignment.Right, VerticalAlignment.Center);
        }

        for (int i = 0; i < Data.Count; i++)
        {
            var stat = Data[i];
            var groupX = padding + (barGroupWidth * i);
            var x = groupX + (barGroupWidth - barWidth * 2 - gap) / 2;

            // Purchase bar (red)
            var pHeight = Math.Max((float)(stat.TotalPurchase / maxValue) * chartHeight, 2f);
            var pY = padding + chartHeight - pHeight;
            var isHoveredP = HoveredIndex == i && HoveredIsPurchase;
            canvas.FillColor = isHoveredP ? Color.FromArgb("#dc2626") : Color.FromArgb("#ef4444");
            canvas.FillRoundedRectangle(x, pY, barWidth, pHeight, 3);
            HitAreas.Add(new BarHitArea(i, true, new RectF(x, pY, barWidth, pHeight), stat));

            // Sales bar (blue)
            var sHeight = Math.Max((float)(stat.TotalSales / maxValue) * chartHeight, 2f);
            var sY = padding + chartHeight - sHeight;
            var isHoveredS = HoveredIndex == i && !HoveredIsPurchase;
            canvas.FillColor = isHoveredS ? Color.FromArgb("#1d4ed8") : Color.FromArgb("#3b82f6");
            canvas.FillRoundedRectangle(x + barWidth + gap, sY, barWidth, sHeight, 3);
            HitAreas.Add(new BarHitArea(i, false, new RectF(x + barWidth + gap, sY, barWidth, sHeight), stat));

            // Date label
            canvas.FontSize = Data.Count > 12 ? 8 : 10;
            canvas.FontColor = Color.FromArgb("#64748b");
            var label = stat.Label ?? stat.Date.ToString("dd/MM");
            canvas.DrawString(label, groupX, padding + chartHeight + 6,
                barGroupWidth, 20, HorizontalAlignment.Center, VerticalAlignment.Top);

            // Tooltip (drawn on top when hovered)
            if (HoveredIndex == i)
            {
                var value = HoveredIsPurchase ? stat.TotalPurchase : stat.TotalSales;
                var tipText = $"{(HoveredIsPurchase ? "Mua" : "Bán")}: {value:N0} đ";
                var tipY = (HoveredIsPurchase ? pY : sY) - 24;
                var tipX = x - 10;
                // Background
                canvas.FillColor = Color.FromArgb("#1e293b");
                canvas.FillRoundedRectangle(tipX, tipY, 130, 20, 6);
                // Text
                canvas.FontColor = Colors.White;
                canvas.FontSize = 10;
                canvas.DrawString(tipText, tipX + 4, tipY + 2, 122, 16, HorizontalAlignment.Center, VerticalAlignment.Center);
            }
        }

        // Legend
        var legendY = height - 18;
        canvas.FillColor = Color.FromArgb("#ef4444");
        canvas.FillRoundedRectangle(padding, legendY, 12, 12, 2);
        canvas.FontSize = 10;
        canvas.FontColor = Color.FromArgb("#64748b");
        canvas.DrawString("Mua vào", padding + 16, legendY, 50, 12, HorizontalAlignment.Left, VerticalAlignment.Center);

        canvas.FillColor = Color.FromArgb("#3b82f6");
        canvas.FillRoundedRectangle(padding + 80, legendY, 12, 12, 2);
        canvas.DrawString("Bán ra", padding + 96, legendY, 50, 12, HorizontalAlignment.Left, VerticalAlignment.Center);
    }

    private static string FormatAmount(decimal amount)
    {
        if (amount >= 1_000_000) return $"{amount / 1_000_000:N1}tr";
        if (amount >= 1_000) return $"{amount / 1_000:N0}k";
        return $"{amount:N0}";
    }
}

/// <summary>
/// Represents a hit area for a single bar in the chart
/// </summary>
public class BarHitArea
{
    public int Index { get; }
    public bool IsPurchase { get; }
    public RectF Rect { get; }
    public DailyStat Stat { get; }

    public BarHitArea(int index, bool isPurchase, RectF rect, DailyStat stat)
    {
        Index = index;
        IsPurchase = isPurchase;
        Rect = rect;
        Stat = stat;
    }
}

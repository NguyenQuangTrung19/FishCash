using FishCash.Models;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Colors = QuestPDF.Helpers.Colors;

namespace FishCash.Services;

/// <summary>
/// Service for generating and exporting invoices (PDF via QuestPDF)
/// </summary>
public class PrintService
{
    private readonly ILogger<PrintService> _logger;

    public PrintService(ILogger<PrintService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generate a professional PDF invoice for trade orders.
    /// Returns PDF as byte array (caller handles file saving via FileSaver).
    /// </summary>
    public byte[] GenerateTradeInvoicePdf(TradingSession session, List<TradeOrder> orders, string exportLabel = "Tất cả")
    {
        var totalPurchase = orders.Where(o => o.OrderType == TradeOrderType.Purchase).Sum(o => o.TotalAmount);
        var totalSales = orders.Where(o => o.OrderType == TradeOrderType.Sale).Sum(o => o.TotalAmount);
        var totalAmount = orders.Sum(o => o.TotalAmount);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(40);
                page.MarginVertical(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                // ═══ HEADER ═══
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Text("CỬA HÀNG FISHCASH").FontSize(20).Bold().FontColor(Colors.Blue.Darken3);
                            left.Item().Text("Hải sản tươi sống — Uy tín chất lượng").FontSize(9).FontColor(Colors.Grey.Darken1);
                        });

                        row.ConstantItem(180).AlignRight().Column(right =>
                        {
                            right.Item().Text("HÓA ĐƠN GIAO DỊCH").FontSize(14).Bold().FontColor(Colors.Blue.Darken3);
                            right.Item().Text($"#{session.Id}").FontSize(10).FontColor(Colors.Grey.Darken1);
                        });
                    });

                    col.Item().PaddingVertical(8).LineHorizontal(2).LineColor(Colors.Blue.Darken3);

                    // Session info
                    col.Item().PaddingBottom(10).Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Text($"Ngày: {session.SessionDate:dd/MM/yyyy HH:mm}").FontSize(10);
                            left.Item().Text($"Loại xuất: {exportLabel}").FontSize(10);
                        });
                        row.RelativeItem().AlignRight().Column(right =>
                        {
                            if (!string.IsNullOrWhiteSpace(session.Note))
                                right.Item().Text($"Ghi chú: {session.Note}").FontSize(10).FontColor(Colors.Grey.Darken1);
                            right.Item().Text($"Số đơn: {orders.Count}").FontSize(10);
                        });
                    });
                });

                // ═══ CONTENT ═══
                page.Content().Column(content =>
                {
                    foreach (var order in orders)
                    {
                        content.Item().PaddingBottom(12).Column(orderCol =>
                        {
                            // Order header
                            var typeLabel = order.OrderType == TradeOrderType.Purchase ? "📥 MUA" : "📤 BÁN";
                            var headerColor = order.OrderType == TradeOrderType.Purchase
                                ? Colors.Red.Darken1 : Colors.Blue.Darken1;

                            orderCol.Item().Background(Colors.Grey.Lighten4).Padding(8).Row(row =>
                            {
                                row.RelativeItem().Text($"{typeLabel}  —  {order.DisplayPartnerName}")
                                    .FontSize(11).Bold().FontColor(headerColor);
                                row.ConstantItem(120).AlignRight().Text($"{order.OrderDate:dd/MM HH:mm}")
                                    .FontSize(9).FontColor(Colors.Grey.Darken1);
                            });

                            // Detail table
                            orderCol.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(30);   // STT
                                    columns.RelativeColumn(3);     // Tên SP
                                    columns.RelativeColumn(1);     // SL
                                    columns.RelativeColumn(0.8f);  // ĐV
                                    columns.RelativeColumn(1.5f);  // Đơn giá
                                    columns.RelativeColumn(1.5f);  // Thành tiền
                                });

                                // Table header
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                                        .Text("STT").FontColor(Colors.White).FontSize(9).Bold().AlignCenter();
                                    header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                                        .Text("Sản phẩm").FontColor(Colors.White).FontSize(9).Bold();
                                    header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                                        .Text("SL").FontColor(Colors.White).FontSize(9).Bold().AlignCenter();
                                    header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                                        .Text("ĐV").FontColor(Colors.White).FontSize(9).Bold().AlignCenter();
                                    header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                                        .Text("Đơn giá").FontColor(Colors.White).FontSize(9).Bold().AlignRight();
                                    header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                                        .Text("Thành tiền").FontColor(Colors.White).FontSize(9).Bold().AlignRight();
                                });

                                // Table rows
                                int stt = 1;
                                foreach (var detail in order.Details)
                                {
                                    var bg = stt % 2 == 0 ? Colors.Grey.Lighten5 : Colors.White;
                                    var productName = detail.Product?.Name ?? "Sản phẩm";

                                    table.Cell().Background(bg).Padding(4).Text($"{stt}").FontSize(9).AlignCenter();
                                    table.Cell().Background(bg).Padding(4).Text(productName).FontSize(9);
                                    table.Cell().Background(bg).Padding(4).Text($"{detail.Quantity:G}").FontSize(9).AlignCenter();
                                    table.Cell().Background(bg).Padding(4).Text(detail.Unit).FontSize(9).AlignCenter();
                                    table.Cell().Background(bg).Padding(4).Text($"{detail.UnitPrice:N0}").FontSize(9).AlignRight();
                                    table.Cell().Background(bg).Padding(4).Text($"{detail.SubTotal:N0}").FontSize(9).AlignRight();
                                    stt++;
                                }
                            });

                            // Order total
                            orderCol.Item().Background(Colors.Grey.Lighten4).PaddingHorizontal(8).PaddingVertical(5)
                                .AlignRight().Text($"Tổng đơn: {order.TotalAmount:N0} VNĐ")
                                .FontSize(10).Bold().FontColor(headerColor);
                        });
                    }

                    // ═══ SUMMARY ═══
                    content.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    content.Item().PaddingTop(10).Column(summary =>
                    {
                        summary.Item().Background(Colors.Blue.Lighten5).Padding(14).Column(box =>
                        {
                            box.Item().Text("TỔNG KẾT").FontSize(13).Bold().FontColor(Colors.Blue.Darken3);
                            box.Item().PaddingTop(6).Row(row =>
                            {
                                row.RelativeItem().Text($"Số đơn hàng:").FontSize(10);
                                row.ConstantItem(120).AlignRight().Text($"{orders.Count}").FontSize(10).Bold();
                            });

                            if (totalPurchase > 0)
                            {
                                box.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Tổng mua vào:").FontSize(10).FontColor(Colors.Red.Darken1);
                                    row.ConstantItem(120).AlignRight().Text($"{totalPurchase:N0} đ")
                                        .FontSize(10).Bold().FontColor(Colors.Red.Darken1);
                                });
                            }
                            if (totalSales > 0)
                            {
                                box.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Tổng bán ra:").FontSize(10).FontColor(Colors.Blue.Darken1);
                                    row.ConstantItem(120).AlignRight().Text($"{totalSales:N0} đ")
                                        .FontSize(10).Bold().FontColor(Colors.Blue.Darken1);
                                });
                            }
                            if (totalPurchase > 0 && totalSales > 0)
                            {
                                box.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                                box.Item().PaddingTop(4).Row(row =>
                                {
                                    row.RelativeItem().Text("LỢI NHUẬN:").FontSize(12).Bold().FontColor(Colors.Green.Darken2);
                                    row.ConstantItem(120).AlignRight().Text($"{totalSales - totalPurchase:N0} đ")
                                        .FontSize(12).Bold().FontColor(Colors.Green.Darken2);
                                });
                            }
                            else
                            {
                                box.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                                box.Item().PaddingTop(4).Row(row =>
                                {
                                    row.RelativeItem().Text("TỔNG CỘNG:").FontSize(12).Bold();
                                    row.ConstantItem(120).AlignRight().Text($"{totalAmount:N0} VNĐ")
                                        .FontSize(12).Bold();
                                });
                            }
                        });
                    });
                });

                // ═══ FOOTER ═══
                page.Footer().Column(footer =>
                {
                    footer.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    footer.Item().PaddingTop(6).Row(row =>
                    {
                        row.RelativeItem().Text("CỬA HÀNG FISHCASH — Cảm ơn Quý Khách!")
                            .FontSize(8).FontColor(Colors.Grey.Darken1);
                        row.RelativeItem().AlignRight().Text(text =>
                        {
                            text.Span("Trang ").FontSize(8).FontColor(Colors.Grey.Darken1);
                            text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                            text.Span(" / ").FontSize(8).FontColor(Colors.Grey.Darken1);
                            text.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                    });
                });
            });
        });

        var bytes = document.GeneratePdf();
        _logger.LogInformation("Trade invoice PDF generated ({Count} orders, {Size} bytes)", orders.Count, bytes.Length);
        return bytes;
    }

    /// <summary>
    /// Generate invoice file for POS orders (kept for backwards compatibility)
    /// </summary>
    public async Task<string> GenerateInvoiceFileAsync(Order order)
    {
        try
        {
            var directory = FileSystem.CacheDirectory;
            var fileName = $"Invoice_{order.Id}_{DateTime.Now:yyyyMMddHHmmss}.txt";
            var path = Path.Combine(directory, fileName);

            using var stream = new StreamWriter(path);
            await stream.WriteLineAsync("=================================");
            await stream.WriteLineAsync("        CỬA HÀNG FISHCASH        ");
            await stream.WriteLineAsync("=================================");
            await stream.WriteLineAsync($"Mã đơn: #{order.Id}");
            await stream.WriteLineAsync($"Ngày: {order.OrderDate:dd/MM/yyyy HH:mm}");
            await stream.WriteLineAsync("---------------------------------");
            
            foreach (var detail in order.OrderDetails)
            {
                await stream.WriteLineAsync($"{detail.Product?.Name ?? "Sản phẩm"}");
                await stream.WriteLineAsync($"  {detail.Quantity} x {detail.UnitPrice:N0} đ = {detail.SubTotal:N0} đ");
            }
            
            await stream.WriteLineAsync("---------------------------------");
            await stream.WriteLineAsync($"TỔNG CỘNG: {order.TotalAmount:N0} VNĐ");
            await stream.WriteLineAsync($"Phương thức: {GetPaymentMethodDisplay(order.PaymentMethod)}");
            await stream.WriteLineAsync("=================================");
            await stream.WriteLineAsync("      Cảm ơn Quý Khách!      ");
            
            _logger.LogInformation("Invoice generated: {Path}", path);
            return path;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice for order: {OrderId}", order.Id);
            throw;
        }
    }

    private static string GetPaymentMethodDisplay(PaymentMethod method) => method switch
    {
        PaymentMethod.Cash => "Tiền mặt",
        PaymentMethod.QrTransfer => "Chuyển khoản QR",
        _ => method.ToString()
    };
}

using FishCash.Models;
using Microsoft.Extensions.Logging;

namespace FishCash.Services;

/// <summary>
/// Service for generating and exporting invoices
/// </summary>
public class PrintService
{
    private readonly ILogger<PrintService> _logger;

    public PrintService(ILogger<PrintService> logger)
    {
        _logger = logger;
    }

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

    /// <summary>
    /// Convert PaymentMethod enum to user-friendly display string
    /// </summary>
    private static string GetPaymentMethodDisplay(PaymentMethod method) => method switch
    {
        PaymentMethod.Cash => "Tiền mặt",
        PaymentMethod.QrTransfer => "Chuyển khoản QR",
        _ => method.ToString()
    };
}

namespace FishCash.Helpers;

/// <summary>
/// Application-wide constants for configuration values
/// </summary>
public static class AppConstants
{
    // Database
    public const string DatabaseName = "fishcash.db";

    // Store Information
    public const string StoreName = "CỬA HÀNG FISHCASH";

    // QR Payment Configuration
    public const string QrBankCode = "970436";          // Vietcombank
    public const string QrAccountNumber = "0987654321"; // Account number
    public const string QrBaseUrl = "https://img.vietqr.io/image";

    // Defaults
    public const string DefaultUnit = "kg";
    public const int RecentOrdersLimit = 50;

    /// <summary>
    /// Generate VietQR payment URL
    /// </summary>
    public static string GetQrPaymentUrl(decimal amount, int orderId)
    {
        return $"{QrBaseUrl}/{QrBankCode}-{QrAccountNumber}-compact.jpg" +
               $"?amount={amount}&addInfo=Thanh toan FishCash don {orderId}";
    }
}

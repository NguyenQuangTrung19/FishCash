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

/// <summary>
/// Type of trading partner
/// </summary>
public enum PartnerType
{
    /// <summary>Nhà cung cấp (chủ ghe, tàu)</summary>
    Supplier,
    /// <summary>Khách mua (nhà hàng, cơ sở chế biến)</summary>
    Buyer
}

/// <summary>
/// Type of trade order within a trading session
/// </summary>
public enum TradeOrderType
{
    /// <summary>Đơn mua vào từ nhà cung cấp</summary>
    Purchase,
    /// <summary>Đơn bán ra cho khách mua</summary>
    Sale
}

/// <summary>
/// Status of a trading session
/// </summary>
public enum SessionStatus
{
    /// <summary>Phiên đang mở, có thể thêm đơn</summary>
    Active,
    /// <summary>Phiên đã hoàn tất, đã tạo hóa đơn</summary>
    Completed
}

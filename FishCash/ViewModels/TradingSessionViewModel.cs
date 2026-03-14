using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishCash.Models;
using FishCash.Services;

namespace FishCash.ViewModels;

/// <summary>
/// Pending cart item for building a trade order (similar to POS cart).
/// Handles unit conversion between kg and tấn (1 tấn = 1000 kg).
/// </summary>
public partial class PendingTradeItem : ObservableObject
{
    [ObservableProperty] private Product product;
    [ObservableProperty] private decimal quantity;
    [ObservableProperty] private string unit = "kg";
    [ObservableProperty] private decimal unitPrice;

    public decimal SubTotal => Quantity * UnitPrice;

    public decimal QuantityInBaseUnit
    {
        get
        {
            var productUnit = Product?.Unit?.ToLower()?.Trim() ?? "kg";
            var enteredUnit = Unit?.ToLower()?.Trim() ?? "kg";
            if (enteredUnit == productUnit) return Quantity;
            if (enteredUnit == "tấn" && productUnit == "kg") return Quantity * 1000;
            if (enteredUnit == "kg" && productUnit == "tấn") return Quantity / 1000;
            return Quantity;
        }
    }

    public decimal DisplayUnitPrice => UnitPrice;

    public PendingTradeItem(Product product, decimal qty, string unit, decimal price)
    {
        Product = product;
        Quantity = qty;
        Unit = unit;
        UnitPrice = price;
    }
}

/// <summary>
/// Wrapper for partner filter list (includes "Tất cả" option)
/// </summary>
public class PartnerFilterItem
{
    public string DisplayName { get; set; } = string.Empty;
    public string? PartnerName { get; set; } // null = "Tất cả"
    public int OrderCount { get; set; }
    public decimal TotalAmount { get; set; }
}

public partial class TradingSessionViewModel : BaseViewModel
{
    private readonly ITradingService _tradingService;
    private readonly IPartnerService _partnerService;
    private readonly IProductService _productService;
    private readonly PrintService _printService;

    public ObservableCollection<TradingSession> Sessions { get; } = new();
    public ObservableCollection<Product> AvailableProducts { get; } = new();
    public ObservableCollection<Partner> AvailablePartners { get; } = new();
    public ObservableCollection<string> UnitOptions { get; } = new() { "kg", "tấn" };

    // ═══ Current Session ═══
    [ObservableProperty] private TradingSession? currentSession;
    [ObservableProperty] private bool isSessionActive;

    public decimal SessionTotalPurchase => CurrentSession?.TotalPurchase ?? 0;
    public decimal SessionTotalSales => CurrentSession?.TotalSales ?? 0;
    public decimal SessionProfit => SessionTotalSales - SessionTotalPurchase;

    public ObservableCollection<TradeOrder> PurchaseOrders { get; } = new();
    public ObservableCollection<TradeOrder> SaleOrders { get; } = new();

    // ═══ Add Order Mode (cart-like flow) ═══
    [ObservableProperty] private bool isAddingOrder;
    [ObservableProperty] private TradeOrderType pendingOrderType = TradeOrderType.Purchase;
    [ObservableProperty] private Partner? selectedPartner;

    public ObservableCollection<PendingTradeItem> PendingItems { get; } = new();
    public decimal PendingTotal => PendingItems.Sum(i => i.SubTotal);

    [ObservableProperty] private bool isItemModalVisible;
    [ObservableProperty] private Product? pendingProduct;
    [ObservableProperty] private string pendingQuantityText = string.Empty;
    [ObservableProperty] private string pendingUnit = "kg";
    [ObservableProperty] private string pendingPriceText = string.Empty;

    public string OrderTypeTitle => PendingOrderType == TradeOrderType.Purchase
        ? "📥 Đơn MUA vào" : "📤 Đơn BÁN ra";
    public string PartnerLabel => PendingOrderType == TradeOrderType.Purchase
        ? "Nhà cung cấp" : "Khách mua";

    // ═══ Partner Selection / Direct Input ═══
    [ObservableProperty] private bool isSelectingPartner;
    [ObservableProperty] private string partnerNameText = string.Empty;

    // ═══ Create Session Modal ═══
    [ObservableProperty] private bool isCreateSessionVisible;
    [ObservableProperty] private string newSessionNote = string.Empty;

    // ═══ Export Menu + Partner Filter ═══
    [ObservableProperty] private bool isExportMenuVisible;
    [ObservableProperty] private bool isPartnerFilterVisible;
    [ObservableProperty] private string partnerFilterTitle = string.Empty;
    public ObservableCollection<PartnerFilterItem> FilterPartners { get; } = new();
    private TradeOrderType? _pendingExportType;

    // ═══ Edit Order Modal ═══
    [ObservableProperty] private bool isEditingOrderVisible;
    [ObservableProperty] private TradeOrder? editingOrder;
    public ObservableCollection<TradeOrderDetail> EditingOrderDetails { get; } = new();
    private bool _isAddingToExistingOrder; // flag: item modal is for existing order
    [ObservableProperty] private bool isPickingProductForEdit; // show product grid for edit mode

    // ═══ Session Summary Modal ═══
    [ObservableProperty] private bool isSummaryVisible;
    [ObservableProperty] private string summaryUpdateTime = string.Empty;
    public ObservableCollection<SessionSummaryItem> SummaryItems { get; } = new();
    public decimal SummaryTotalPurchase => SummaryItems.Sum(s => s.PurchaseAmount);
    public decimal SummaryTotalSales => SummaryItems.Sum(s => s.SalesAmount);
    public decimal SummaryTotalProfit => SummaryTotalSales - SummaryTotalPurchase;

    public TradingSessionViewModel(ITradingService tradingService, IPartnerService partnerService,
        IProductService productService, PrintService printService)
    {
        _tradingService = tradingService;
        _partnerService = partnerService;
        _productService = productService;
        _printService = printService;
        Title = "Bán hàng";
    }

    // ═══ Load ═══

    [RelayCommand]
    public async Task LoadSessionsAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            Sessions.Clear();
            var sessions = await _tradingService.GetSessionsAsync();
            foreach (var s in sessions) Sessions.Add(s);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể tải phiên GD: {ex.Message}", "OK");
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        try
        {
            AvailableProducts.Clear();
            var products = await _productService.GetProductsAsync();
            foreach (var p in products) AvailableProducts.Add(p);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể tải sản phẩm: {ex.Message}", "OK");
        }
    }

    // ═══ Create Session ═══
    [RelayCommand] public void ShowCreateSession() { NewSessionNote = string.Empty; IsCreateSessionVisible = true; }
    [RelayCommand] public void CancelCreateSession() => IsCreateSessionVisible = false;

    [RelayCommand]
    public async Task CreateSessionAsync()
    {
        try
        {
            var session = await _tradingService.CreateSessionAsync(NewSessionNote);
            IsCreateSessionVisible = false;
            CurrentSession = await _tradingService.GetSessionByIdAsync(session.Id);
            IsSessionActive = true;
            RefreshSessionData();
            await LoadSessionsAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể tạo phiên: {ex.Message}", "OK");
        }
    }

    // ═══ Open/Close Session ═══
    [RelayCommand]
    public async Task OpenSessionAsync(TradingSession session)
    {
        if (session == null) return;
        try
        {
            CurrentSession = await _tradingService.GetSessionByIdAsync(session.Id);
            IsSessionActive = true;
            RefreshSessionData();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể mở phiên: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public void CloseSession()
    {
        CurrentSession = null;
        IsSessionActive = false;
        PurchaseOrders.Clear();
        SaleOrders.Clear();
    }

    // ═══ Save Session ═══
    [RelayCommand]
    public async Task SaveSessionAsync()
    {
        if (CurrentSession == null) return;
        try
        {
            await _tradingService.SaveSessionAsync(CurrentSession.Id, CurrentSession.Note);
            await LoadSessionsAsync();
            await Shell.Current.DisplayAlert("Thành công", "Đã lưu phiên giao dịch!", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Lỗi lưu phiên: {ex.Message}", "OK");
        }
    }

    // ═══ Export Invoice ═══

    [RelayCommand] public void ShowExportMenu() => IsExportMenuVisible = true;
    [RelayCommand] public void HideExportMenu() => IsExportMenuVisible = false;
    [RelayCommand] public void HidePartnerFilter() => IsPartnerFilterVisible = false;

    [RelayCommand]
    public async Task ExportAllAsync()
    {
        IsExportMenuVisible = false;
        if (CurrentSession == null) return;
        var allOrders = PurchaseOrders.Concat(SaleOrders).ToList();
        if (allOrders.Count == 0)
        {
            await Shell.Current.DisplayAlert("Thông báo", "Chưa có đơn hàng nào để xuất", "OK");
            return;
        }
        await SavePdfAsync(allOrders, "Tất cả đơn");
    }

    [RelayCommand]
    public void ExportPurchases()
    {
        IsExportMenuVisible = false;
        if (CurrentSession == null) return;
        if (PurchaseOrders.Count == 0)
        {
            Shell.Current.DisplayAlert("Thông báo", "Chưa có đơn mua nào", "OK");
            return;
        }
        ShowPartnerFilterForType(TradeOrderType.Purchase);
    }

    [RelayCommand]
    public void ExportSales()
    {
        IsExportMenuVisible = false;
        if (CurrentSession == null) return;
        if (SaleOrders.Count == 0)
        {
            Shell.Current.DisplayAlert("Thông báo", "Chưa có đơn bán nào", "OK");
            return;
        }
        ShowPartnerFilterForType(TradeOrderType.Sale);
    }

    private void ShowPartnerFilterForType(TradeOrderType type)
    {
        _pendingExportType = type;
        var orders = type == TradeOrderType.Purchase ? PurchaseOrders : SaleOrders;
        var typeLabel = type == TradeOrderType.Purchase ? "mua" : "bán";

        PartnerFilterTitle = type == TradeOrderType.Purchase
            ? "📥 Xuất đơn mua — Chọn đối tác" : "📤 Xuất đơn bán — Chọn đối tác";

        FilterPartners.Clear();

        // "Tất cả" option
        FilterPartners.Add(new PartnerFilterItem
        {
            DisplayName = $"📋 Tất cả đơn {typeLabel}",
            PartnerName = null,
            OrderCount = orders.Count,
            TotalAmount = orders.Sum(o => o.TotalAmount)
        });

        // Group by partner
        var grouped = orders.GroupBy(o => o.DisplayPartnerName ?? "Không rõ")
            .OrderByDescending(g => g.Sum(o => o.TotalAmount));
        foreach (var group in grouped)
        {
            FilterPartners.Add(new PartnerFilterItem
            {
                DisplayName = group.Key,
                PartnerName = group.Key,
                OrderCount = group.Count(),
                TotalAmount = group.Sum(o => o.TotalAmount)
            });
        }

        IsPartnerFilterVisible = true;
    }

    [RelayCommand]
    public async Task SelectPartnerFilterAsync(PartnerFilterItem item)
    {
        IsPartnerFilterVisible = false;
        if (CurrentSession == null || item == null) return;

        var orders = _pendingExportType == TradeOrderType.Purchase ? PurchaseOrders : SaleOrders;
        var typeLabel = _pendingExportType == TradeOrderType.Purchase ? "Đơn mua" : "Đơn bán";

        List<TradeOrder> filtered;
        string label;

        if (item.PartnerName == null)
        {
            // Tất cả
            filtered = orders.ToList();
            label = $"{typeLabel} — Tất cả";
        }
        else
        {
            // Lọc theo partner
            filtered = orders.Where(o => o.DisplayPartnerName == item.PartnerName).ToList();
            label = $"{typeLabel} — {item.PartnerName}";
        }

        if (filtered.Count == 0) return;
        await SavePdfAsync(filtered, label);
    }

    [RelayCommand]
    public async Task ExportSingleOrderAsync(TradeOrder order)
    {
        if (CurrentSession == null || order == null) return;
        var typeLabel = order.OrderType == TradeOrderType.Purchase ? "Đơn mua" : "Đơn bán";
        await SavePdfAsync(new List<TradeOrder> { order }, $"{typeLabel} — {order.DisplayPartnerName}");
    }

    /// <summary>
    /// Generate PDF and let user choose save location via FileSaver
    /// </summary>
    private async Task SavePdfAsync(List<TradeOrder> orders, string label)
    {
        try
        {
            var pdfBytes = _printService.GenerateTradeInvoicePdf(CurrentSession!, orders, label);
            var fileName = $"HoaDon_Phien{CurrentSession!.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            using var stream = new MemoryStream(pdfBytes);
            var result = await FileSaver.Default.SaveAsync(fileName, stream, CancellationToken.None);

            if (result.IsSuccessful)
            {
                await Shell.Current.DisplayAlert("Xuất hóa đơn", $"Đã lưu hóa đơn PDF thành công!\n📁 {result.FilePath}", "OK");
            }
            else if (result.Exception != null)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Lỗi lưu file: {result.Exception.Message}", "OK");
            }
            // If user cancelled the save dialog, do nothing
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Lỗi xuất hóa đơn: {ex.Message}", "OK");
        }
    }

    // ═══ Session Summary ═══

    [RelayCommand]
    public void ShowSessionSummary()
    {
        if (CurrentSession?.TradeOrders == null) return;
        SummaryItems.Clear();

        // Group all order details by product, convert to kg
        var allDetails = CurrentSession.TradeOrders
            .SelectMany(o => o.Details.Select(d => new { Detail = d, o.OrderType }))
            .ToList();

        var grouped = allDetails
            .GroupBy(x => x.Detail.Product?.Name ?? "?")
            .OrderBy(g => g.Key);

        foreach (var g in grouped)
        {
            var item = new SessionSummaryItem { ProductName = g.Key };
            foreach (var x in g)
            {
                var qtyKg = ConvertToKg(x.Detail.Quantity, x.Detail.Unit ?? x.Detail.Product?.Unit ?? "kg");
                if (x.OrderType == TradeOrderType.Purchase)
                {
                    item.PurchaseQtyKg += qtyKg;
                    item.PurchaseAmount += x.Detail.SubTotal;
                }
                else
                {
                    item.SalesQtyKg += qtyKg;
                    item.SalesAmount += x.Detail.SubTotal;
                }
            }
            SummaryItems.Add(item);
        }

        SummaryUpdateTime = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
        OnPropertyChanged(nameof(SummaryTotalPurchase));
        OnPropertyChanged(nameof(SummaryTotalSales));
        OnPropertyChanged(nameof(SummaryTotalProfit));
        IsSummaryVisible = true;
    }

    [RelayCommand]
    public void CloseSummary() { IsSummaryVisible = false; }

    private static decimal ConvertToKg(decimal qty, string unit)
    {
        var u = unit?.ToLower()?.Trim() ?? "kg";
        if (u == "tấn") return qty * 1000;
        return qty; // already kg or unknown
    }

    // ═══ Edit Existing Order ═══

    [RelayCommand]
    public async Task EditOrderAsync(TradeOrder order)
    {
        if (order == null || CurrentSession == null) return;
        // Store reference to order being edited
        EditingOrder = order;
        _isAddingToExistingOrder = true;
        PendingOrderType = order.OrderType;

        // Set partner info from existing order
        SelectedPartner = order.Partner;
        PartnerNameText = order.PartnerName ?? order.DisplayPartnerName ?? string.Empty;

        // Pre-fill cart with existing items
        PendingItems.Clear();
        foreach (var d in order.Details)
        {
            if (d.Product != null)
                PendingItems.Add(new PendingTradeItem(d.Product, d.Quantity, d.Unit ?? d.Product.Unit, d.UnitPrice));
        }
        OnPropertyChanged(nameof(PendingTotal));
        OnPropertyChanged(nameof(OrderTypeTitle));
        OnPropertyChanged(nameof(PartnerLabel));

        // Load products and go straight to POS cart (Layer 3), skip partner selection
        await LoadProductsAsync();
        IsAddingOrder = true;
    }

    // ═══ Add Order Flow (cart-like) ═══

    [RelayCommand]
    public async Task StartAddPurchaseAsync()
    {
        PendingOrderType = TradeOrderType.Purchase;
        await StartAddOrderFlow();
    }

    [RelayCommand]
    public async Task StartAddSaleAsync()
    {
        PendingOrderType = TradeOrderType.Sale;
        await StartAddOrderFlow();
    }

    private async Task StartAddOrderFlow()
    {
        try
        {
            AvailablePartners.Clear();
            var filterType = PendingOrderType == TradeOrderType.Purchase ? PartnerType.Supplier : PartnerType.Buyer;
            var partners = await _partnerService.GetPartnersAsync(filterType);
            foreach (var p in partners) AvailablePartners.Add(p);

            SelectedPartner = null;
            PartnerNameText = string.Empty;
            PendingItems.Clear();
            OnPropertyChanged(nameof(PendingTotal));
            OnPropertyChanged(nameof(OrderTypeTitle));
            OnPropertyChanged(nameof(PartnerLabel));
            IsSelectingPartner = true;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể tải đối tác: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task ConfirmPartnerAsync()
    {
        var hasPartner = SelectedPartner != null;
        var hasName = !string.IsNullOrWhiteSpace(PartnerNameText);
        if (!hasPartner && !hasName)
        {
            await Shell.Current.DisplayAlert("Lỗi", "Vui lòng nhập tên hoặc chọn đối tác", "OK");
            return;
        }
        if (hasPartner && !hasName) PartnerNameText = SelectedPartner!.Name;
        IsSelectingPartner = false;
        IsAddingOrder = true;
        await LoadProductsAsync();
    }

    [RelayCommand]
    public void CancelAddOrder()
    {
        IsAddingOrder = false;
        IsSelectingPartner = false;
        PendingItems.Clear();
        EditingOrder = null;
        _isAddingToExistingOrder = false;
        OnPropertyChanged(nameof(PendingTotal));
    }

    // ═══ Product → Cart ═══
    [RelayCommand]
    public void AddProductToOrder(Product product)
    {
        if (product == null) return;
        PendingProduct = product;
        PendingQuantityText = string.Empty;
        PendingUnit = product.Unit;
        PendingPriceText = product.Price > 0 ? product.Price.ToString("N0").Replace(",", ".") : string.Empty;
        IsItemModalVisible = true;
    }

    partial void OnPendingUnitChanged(string? oldValue, string newValue)
    {
        if (PendingProduct == null || PendingProduct.Price <= 0) return;
        var productUnit = PendingProduct.Unit?.ToLower()?.Trim() ?? "kg";
        var enteredUnit = newValue?.ToLower()?.Trim() ?? "kg";
        decimal displayPrice = PendingProduct.Price;
        if (enteredUnit == "tấn" && productUnit == "kg") displayPrice = PendingProduct.Price * 1000;
        else if (enteredUnit == "kg" && productUnit == "tấn") displayPrice = PendingProduct.Price / 1000;
        PendingPriceText = displayPrice.ToString("N0").Replace(",", ".");
    }

    [RelayCommand]
    public void CancelItemModal()
    {
        IsItemModalVisible = false;
    }

    [RelayCommand]
    public async Task ConfirmItemAsync()
    {
        if (PendingProduct == null) return;
        var cleanQty = PendingQuantityText.Replace(",", ".");
        if (!decimal.TryParse(cleanQty, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out decimal qty) || qty <= 0)
        {
            await Shell.Current.DisplayAlert("Lỗi", "Số lượng không hợp lệ", "OK");
            return;
        }
        var cleanPrice = PendingPriceText.Replace(".", "").Replace(",", "");
        if (!decimal.TryParse(cleanPrice, out decimal price) || price <= 0)
        {
            await Shell.Current.DisplayAlert("Lỗi", "Đơn giá không hợp lệ", "OK");
            return;
        }

        // Merge if same product already in cart (convert to base unit if different units)
        var existing = PendingItems.FirstOrDefault(i => i.Product.Id == PendingProduct.Id);
        if (existing != null)
        {
            // Convert both to base unit (product's default unit)
            var baseUnit = PendingProduct.Unit?.ToLower()?.Trim() ?? "kg";
            var existingBaseQty = ConvertToBaseUnit(existing.Quantity, existing.Unit, baseUnit);
            var existingBasePrice = ConvertPriceToBaseUnit(existing.UnitPrice, existing.Unit, baseUnit);
            var newBaseQty = ConvertToBaseUnit(qty, PendingUnit, baseUnit);
            var newBasePrice = ConvertPriceToBaseUnit(price, PendingUnit, baseUnit);

            var totalBaseQty = existingBaseQty + newBaseQty;
            // Weighted average price in base unit
            var avgBasePrice = (existingBaseQty * existingBasePrice + newBaseQty * newBasePrice) / totalBaseQty;

            var index = PendingItems.IndexOf(existing);
            PendingItems[index] = new PendingTradeItem(PendingProduct, totalBaseQty, PendingProduct.Unit, avgBasePrice);
        }
        else
        {
            PendingItems.Add(new PendingTradeItem(PendingProduct, qty, PendingUnit, price));
        }
        OnPropertyChanged(nameof(PendingTotal));
        IsItemModalVisible = false;
    }

    [RelayCommand]
    public void RemovePendingItem(PendingTradeItem item)
    {
        if (item == null) return;
        PendingItems.Remove(item);
        OnPropertyChanged(nameof(PendingTotal));
    }

    // ═══ Confirm Order ═══
    [RelayCommand]
    public async Task ConfirmOrderAsync()
    {
        if (CurrentSession == null || PendingItems.Count == 0)
        {
            await Shell.Current.DisplayAlert("Lỗi", "Giỏ hàng trống", "OK");
            return;
        }

        try
        {
            var details = PendingItems.Select(item => new TradeOrderDetail
            {
                ProductId = item.Product.Id,
                Quantity = item.Quantity,
                Unit = item.Unit,
                UnitPrice = item.UnitPrice,
                SubTotal = item.SubTotal
            }).ToList();

            if (_isAddingToExistingOrder && EditingOrder != null)
            {
                // Update existing order
                await _tradingService.UpdateTradeOrderAsync(EditingOrder.Id, details);
            }
            else
            {
                // Create new order
                var partnerName = PartnerNameText?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(partnerName) && SelectedPartner == null)
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Chưa có thông tin đối tác", "OK");
                    return;
                }
                await _tradingService.AddTradeOrderAsync(CurrentSession.Id, SelectedPartner?.Id,
                    partnerName, PendingOrderType, details);
            }

            CurrentSession = await _tradingService.GetSessionByIdAsync(CurrentSession.Id);
            RefreshSessionData();
            IsAddingOrder = false;
            PendingItems.Clear();
            EditingOrder = null;
            _isAddingToExistingOrder = false;
            OnPropertyChanged(nameof(PendingTotal));
            await LoadSessionsAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể lưu đơn: {ex.Message}", "OK");
        }
    }

    // ═══ Remove Order ═══
    [RelayCommand]
    public async Task RemoveOrderAsync(TradeOrder order)
    {
        if (order == null || CurrentSession == null) return;
        bool confirm = await Shell.Current.DisplayAlert("Xác nhận",
            $"Xóa đơn của \"{order.DisplayPartnerName}\" ({order.TotalAmount:N0} đ)?", "Xóa", "Hủy");
        if (!confirm) return;
        try
        {
            await _tradingService.RemoveTradeOrderAsync(order.Id);
            CurrentSession = await _tradingService.GetSessionByIdAsync(CurrentSession.Id);
            RefreshSessionData();
            await LoadSessionsAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Lỗi: {ex.Message}", "OK");
        }
    }

    // ═══ Helpers ═══
    private void RefreshSessionData()
    {
        PurchaseOrders.Clear();
        SaleOrders.Clear();
        if (CurrentSession?.TradeOrders != null)
        {
            foreach (var order in CurrentSession.TradeOrders)
            {
                if (order.OrderType == TradeOrderType.Purchase) PurchaseOrders.Add(order);
                else SaleOrders.Add(order);
            }
        }
        OnPropertyChanged(nameof(SessionTotalPurchase));
        OnPropertyChanged(nameof(SessionTotalSales));
        OnPropertyChanged(nameof(SessionProfit));
    }

    /// <summary>Convert quantity to base unit (product's default unit)</summary>
    private static decimal ConvertToBaseUnit(decimal quantity, string? fromUnit, string baseUnit)
    {
        var from = fromUnit?.ToLower()?.Trim() ?? baseUnit;
        if (from == baseUnit) return quantity;
        if (from == "tấn" && baseUnit == "kg") return quantity * 1000;
        if (from == "kg" && baseUnit == "tấn") return quantity / 1000;
        return quantity;
    }

    /// <summary>Convert unit price to base unit price</summary>
    private static decimal ConvertPriceToBaseUnit(decimal unitPrice, string? fromUnit, string baseUnit)
    {
        var from = fromUnit?.ToLower()?.Trim() ?? baseUnit;
        if (from == baseUnit) return unitPrice;
        // Price is inverse of quantity: 1 tấn costs X → 1 kg costs X/1000
        if (from == "tấn" && baseUnit == "kg") return unitPrice / 1000;
        if (from == "kg" && baseUnit == "tấn") return unitPrice * 1000;
        return unitPrice;
    }
}

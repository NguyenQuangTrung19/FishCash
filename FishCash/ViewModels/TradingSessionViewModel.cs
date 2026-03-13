using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishCash.Models;
using FishCash.Services;

namespace FishCash.ViewModels;

public partial class TradingSessionViewModel : BaseViewModel
{
    private readonly ITradingService _tradingService;
    private readonly IPartnerService _partnerService;
    private readonly IProductService _productService;

    public ObservableCollection<TradingSession> Sessions { get; } = new();
    public ObservableCollection<Partner> AvailablePartners { get; } = new();
    public ObservableCollection<Product> AvailableProducts { get; } = new();
    public ObservableCollection<string> UnitOptions { get; } = new() { "kg", "tấn" };

    // ═══ Current Session ═══
    [ObservableProperty] private TradingSession? currentSession;
    [ObservableProperty] private bool isSessionActive;

    // Computed from current session
    public decimal SessionTotalPurchase => CurrentSession?.TotalPurchase ?? 0;
    public decimal SessionTotalSales => CurrentSession?.TotalSales ?? 0;
    public decimal SessionProfit => SessionTotalSales - SessionTotalPurchase;

    public ObservableCollection<TradeOrder> PurchaseOrders { get; } = new();
    public ObservableCollection<TradeOrder> SaleOrders { get; } = new();

    // ═══ Add Order Modal ═══
    [ObservableProperty] private bool isAddOrderModalVisible;
    [ObservableProperty] private TradeOrderType pendingOrderType = TradeOrderType.Purchase;
    [ObservableProperty] private Partner? selectedPartner;
    [ObservableProperty] private Product? selectedProduct;
    [ObservableProperty] private string pendingQuantityText = string.Empty;
    [ObservableProperty] private string pendingUnit = "kg";
    [ObservableProperty] private string pendingPriceText = string.Empty;
    [ObservableProperty] private string pendingNote = string.Empty;

    public string ModalTitle => PendingOrderType == TradeOrderType.Purchase 
        ? "📥 Thêm đơn MUA" : "📤 Thêm đơn BÁN";
    public string PartnerLabel => PendingOrderType == TradeOrderType.Purchase 
        ? "Nhà cung cấp" : "Khách mua";

    // ═══ Session Detail Modal ═══
    [ObservableProperty] private bool isSessionDetailVisible;
    [ObservableProperty] private TradingSession? detailSession;

    // ═══ Create Session Modal ═══
    [ObservableProperty] private bool isCreateSessionVisible;
    [ObservableProperty] private string newSessionNote = string.Empty;

    public TradingSessionViewModel(ITradingService tradingService, IPartnerService partnerService, IProductService productService)
    {
        _tradingService = tradingService;
        _partnerService = partnerService;
        _productService = productService;
        Title = "Phiên giao dịch";
    }

    [RelayCommand]
    public async Task LoadSessionsAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            Sessions.Clear();
            var sessions = await _tradingService.GetSessionsAsync();
            foreach (var s in sessions)
                Sessions.Add(s);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể tải phiên GD: {ex.Message}", "OK");
        }
        finally { IsBusy = false; }
    }

    // ═══ Create Session ═══

    [RelayCommand]
    public void ShowCreateSession()
    {
        NewSessionNote = string.Empty;
        IsCreateSessionVisible = true;
    }

    [RelayCommand]
    public void CancelCreateSession()
    {
        IsCreateSessionVisible = false;
    }

    [RelayCommand]
    public async Task CreateSessionAsync()
    {
        try
        {
            var session = await _tradingService.CreateSessionAsync(NewSessionNote);
            IsCreateSessionVisible = false;
            
            // Open the new session
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

    [RelayCommand]
    public async Task CompleteSessionAsync()
    {
        if (CurrentSession == null) return;

        bool confirm = await Shell.Current.DisplayAlert("Hoàn tất phiên",
            $"Tổng mua: {SessionTotalPurchase:N0} đ\nTổng bán: {SessionTotalSales:N0} đ\nLợi nhuận: {SessionProfit:N0} đ\n\nHoàn tất phiên giao dịch này?",
            "Hoàn tất", "Hủy");
        if (!confirm) return;

        try
        {
            await _tradingService.CompleteSessionAsync(CurrentSession.Id);
            CurrentSession = await _tradingService.GetSessionByIdAsync(CurrentSession.Id);
            RefreshSessionData();
            await LoadSessionsAsync();
            await Shell.Current.DisplayAlert("Thành công", "Phiên giao dịch đã hoàn tất!", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Lỗi: {ex.Message}", "OK");
        }
    }

    // ═══ Add Trade Order ═══

    [RelayCommand]
    public async Task ShowAddPurchaseAsync()
    {
        PendingOrderType = TradeOrderType.Purchase;
        await PrepareOrderModal();
    }

    [RelayCommand]
    public async Task ShowAddSaleAsync()
    {
        PendingOrderType = TradeOrderType.Sale;
        await PrepareOrderModal();
    }

    private async Task PrepareOrderModal()
    {
        try
        {
            AvailablePartners.Clear();
            AvailableProducts.Clear();

            var filterType = PendingOrderType == TradeOrderType.Purchase ? PartnerType.Supplier : PartnerType.Buyer;
            var partners = await _partnerService.GetPartnersAsync(filterType);
            foreach (var p in partners) AvailablePartners.Add(p);

            var products = await _productService.GetProductsAsync();
            foreach (var p in products) AvailableProducts.Add(p);

            SelectedPartner = null;
            SelectedProduct = null;
            PendingQuantityText = string.Empty;
            PendingUnit = "kg";
            PendingPriceText = string.Empty;
            PendingNote = string.Empty;

            OnPropertyChanged(nameof(ModalTitle));
            OnPropertyChanged(nameof(PartnerLabel));
            IsAddOrderModalVisible = true;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể mở form: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public void CancelAddOrder()
    {
        IsAddOrderModalVisible = false;
    }

    [RelayCommand]
    public async Task ConfirmAddOrderAsync()
    {
        if (CurrentSession == null || SelectedPartner == null || SelectedProduct == null)
        {
            await Shell.Current.DisplayAlert("Lỗi", "Vui lòng chọn đối tác và sản phẩm", "OK");
            return;
        }

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

        try
        {
            var detail = new TradeOrderDetail
            {
                ProductId = SelectedProduct.Id,
                Quantity = qty,
                Unit = PendingUnit,
                UnitPrice = price,
                SubTotal = qty * price
            };

            await _tradingService.AddTradeOrderAsync(
                CurrentSession.Id, SelectedPartner.Id, PendingOrderType,
                new List<TradeOrderDetail> { detail }, PendingNote);

            // Reload session
            CurrentSession = await _tradingService.GetSessionByIdAsync(CurrentSession.Id);
            RefreshSessionData();
            IsAddOrderModalVisible = false;
            await LoadSessionsAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể thêm đơn: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task RemoveOrderAsync(TradeOrder order)
    {
        if (order == null || CurrentSession == null) return;
        bool confirm = await Shell.Current.DisplayAlert("Xác nhận",
            $"Xóa đơn của \"{order.Partner?.Name}\" ({order.TotalAmount:N0} đ)?", "Xóa", "Hủy");
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
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể xóa: {ex.Message}", "OK");
        }
    }

    // ═══ Session Detail ═══

    [RelayCommand]
    public async Task ViewSessionDetailAsync(TradingSession session)
    {
        if (session == null) return;
        try
        {
            DetailSession = await _tradingService.GetSessionByIdAsync(session.Id);
            IsSessionDetailVisible = true;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể tải chi tiết: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public void CloseSessionDetail()
    {
        IsSessionDetailVisible = false;
        DetailSession = null;
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
                if (order.OrderType == TradeOrderType.Purchase)
                    PurchaseOrders.Add(order);
                else
                    SaleOrders.Add(order);
            }
        }

        OnPropertyChanged(nameof(SessionTotalPurchase));
        OnPropertyChanged(nameof(SessionTotalSales));
        OnPropertyChanged(nameof(SessionProfit));
    }
}

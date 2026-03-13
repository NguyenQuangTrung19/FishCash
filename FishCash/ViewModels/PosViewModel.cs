using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishCash.Models;
using FishCash.Services;

namespace FishCash.ViewModels;

public partial class PosViewModel : BaseViewModel
{
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    public CartService CartService { get; }

    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<string> DisplayUnitOptions { get; } = new() { "kg", "tấn" };
    public ObservableCollection<string> EntryUnitOptions { get; } = new() { "kg", "tấn" };

    public decimal TotalAmount => CartService.GetTotalAmount();

    // ═══ Display Unit ═══
    [ObservableProperty]
    private string displayUnit = "kg";

    partial void OnDisplayUnitChanged(string value)
    {
        CartService.UpdateDisplayUnit(value);
        OnPropertyChanged(nameof(TotalAmount));
    }

    // ═══ Quantity Input Modal ═══
    [ObservableProperty]
    private bool isQuantityModalVisible;

    [ObservableProperty]
    private Product? pendingProduct;

    [ObservableProperty]
    private string pendingQuantityText = string.Empty;

    [ObservableProperty]
    private string pendingUnit = "kg";

    [ObservableProperty]
    private bool isEditingCartItem;

    private CartItem? _editingCartItem;

    public PosViewModel(IProductService productService, CartService cartService, IOrderService orderService)
    {
        _productService = productService;
        CartService = cartService;
        _orderService = orderService;
        Title = "Máy Tính Tiền";

        CartService.Items.CollectionChanged += (s, e) => OnPropertyChanged(nameof(TotalAmount));
    }

    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            Products.Clear();
            var products = await _productService.GetProductsAsync();
            foreach (var product in products)
                Products.Add(product);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể tải sản phẩm: {ex.Message}", "OK");
        }
        finally { IsBusy = false; }
    }

    /// <summary>
    /// Open quantity modal for adding a NEW product
    /// </summary>
    [RelayCommand]
    public void AddToCart(Product product)
    {
        if (product == null) return;
        PendingProduct = product;
        PendingQuantityText = string.Empty;
        PendingUnit = product.Unit;
        IsEditingCartItem = false;
        _editingCartItem = null;
        IsQuantityModalVisible = true;
    }

    /// <summary>
    /// Open quantity modal for EDITING an existing cart item
    /// </summary>
    [RelayCommand]
    public void EditCartItem(CartItem cartItem)
    {
        if (cartItem == null) return;
        PendingProduct = cartItem.Product;
        PendingQuantityText = cartItem.EnteredQuantity.ToString("G");
        PendingUnit = cartItem.EnteredUnit;
        IsEditingCartItem = true;
        _editingCartItem = cartItem;
        IsQuantityModalVisible = true;
    }

    [RelayCommand]
    public void CancelQuantityModal()
    {
        IsQuantityModalVisible = false;
        PendingProduct = null;
        _editingCartItem = null;
    }

    [RelayCommand]
    public async Task ConfirmAddToCartAsync()
    {
        if (PendingProduct == null) return;

        var cleanText = PendingQuantityText.Replace(",", ".");
        if (!decimal.TryParse(cleanText, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out decimal qty) || qty <= 0)
        {
            await Shell.Current.DisplayAlert("Lỗi", "Số lượng không hợp lệ. Vui lòng nhập số lớn hơn 0.", "OK");
            return;
        }

        if (IsEditingCartItem && _editingCartItem != null)
        {
            _editingCartItem.EnteredQuantity = qty;
            _editingCartItem.EnteredUnit = PendingUnit;
            _editingCartItem.RefreshConversions();
        }
        else
        {
            CartService.AddToCart(PendingProduct, qty, PendingUnit);
        }

        OnPropertyChanged(nameof(TotalAmount));
        IsQuantityModalVisible = false;
        PendingProduct = null;
        _editingCartItem = null;
    }

    [RelayCommand]
    public void RemoveFromCart(Product product)
    {
        if (product == null) return;
        CartService.RemoveFromCart(product);
        OnPropertyChanged(nameof(TotalAmount));
    }

    [RelayCommand]
    public async Task CheckoutAsync()
    {
        if (CartService.Items.Count == 0)
        {
            await Shell.Current.DisplayAlert("Giỏ hàng rỗng", "Vui lòng chọn sản phẩm trước khi thanh toán.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            var orderDetails = CartService.GetOrderDetails();
            var order = await _orderService.CreateOrderAsync(orderDetails, PaymentMethod.QrTransfer);
            
            CartService.ClearCart();
            OnPropertyChanged(nameof(TotalAmount));

            await Shell.Current.GoToAsync(nameof(Views.CheckoutPage), new Dictionary<string, object>
            {
                { "Order", order }
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Lỗi thanh toán: {ex.Message}", "OK");
        }
        finally { IsBusy = false; }
    }
}

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
    
    // Binding property for TotalAmount to auto-update UI when cart changes
    public decimal TotalAmount => CartService.GetTotalAmount();

    public PosViewModel(IProductService productService, CartService cartService, IOrderService orderService)
    {
        _productService = productService;
        CartService = cartService;
        _orderService = orderService;
        Title = "Máy Tính Tiền";

        // Listen to changes in CartService items to update TotalAmount
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
            {
                Products.Add(product);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể tải sản phẩm: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void AddToCart(Product product)
    {
        if (product == null) return;
        CartService.AddToCart(product, 1);
        OnPropertyChanged(nameof(TotalAmount));
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
        finally
        {
            IsBusy = false;
        }
    }
}

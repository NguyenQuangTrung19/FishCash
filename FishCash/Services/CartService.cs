using System.Collections.ObjectModel;
using FishCash.Models;

namespace FishCash.Services;

/// <summary>
/// Service for managing the shopping cart (Singleton in DI)
/// </summary>
public class CartService
{
    public ObservableCollection<CartItem> Items { get; } = new();

    public void AddToCart(Product product, decimal quantity = 1)
    {
        if (product == null) return;
        if (quantity <= 0) return;

        var existingItem = Items.FirstOrDefault(i => i.Product.Id == product.Id);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            Items.Add(new CartItem { Product = product, Quantity = quantity });
        }
    }

    public void RemoveFromCart(Product product)
    {
        if (product == null) return;

        var item = Items.FirstOrDefault(i => i.Product.Id == product.Id);
        if (item != null)
        {
            Items.Remove(item);
        }
    }

    public void ClearCart()
    {
        Items.Clear();
    }

    public decimal GetTotalAmount()
    {
        return Items.Sum(i => i.SubTotal);
    }

    public List<OrderDetail> GetOrderDetails()
    {
        return Items.Select(i => new OrderDetail
        {
            ProductId = i.Product.Id,
            Quantity = i.Quantity,
            UnitPrice = i.Product.Price,
            SubTotal = i.SubTotal
        }).ToList();
    }
}

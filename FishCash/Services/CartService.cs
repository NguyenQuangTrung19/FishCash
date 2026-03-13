using System.Collections.ObjectModel;
using FishCash.Models;

namespace FishCash.Services;

/// <summary>
/// Service for managing the shopping cart (Singleton in DI).
/// Supports unit conversion between kg and tấn.
/// </summary>
public class CartService
{
    public ObservableCollection<CartItem> Items { get; } = new();

    /// <summary>
    /// The common display unit for POS (defaults to "kg")
    /// </summary>
    public string DisplayUnit { get; set; } = "kg";

    public void AddToCart(Product product, decimal quantity, string enteredUnit)
    {
        if (product == null || quantity <= 0) return;

        var existingItem = Items.FirstOrDefault(i => i.Product.Id == product.Id);
        if (existingItem != null)
        {
            // Combine: convert new quantity to same entered unit, then add
            existingItem.EnteredQuantity += quantity;
        }
        else
        {
            Items.Add(new CartItem
            {
                Product = product,
                EnteredQuantity = quantity,
                EnteredUnit = enteredUnit,
                DisplayUnit = DisplayUnit
            });
        }
    }

    public void RemoveFromCart(Product product)
    {
        if (product == null) return;
        var item = Items.FirstOrDefault(i => i.Product.Id == product.Id);
        if (item != null) Items.Remove(item);
    }

    public void ClearCart()
    {
        Items.Clear();
    }

    public decimal GetTotalAmount()
    {
        return Items.Sum(i => i.SubTotal);
    }

    /// <summary>
    /// Update display unit for all cart items and refresh computed values
    /// </summary>
    public void UpdateDisplayUnit(string newUnit)
    {
        DisplayUnit = newUnit;
        foreach (var item in Items)
        {
            item.DisplayUnit = newUnit;
            item.RefreshConversions();
        }
    }

    public List<OrderDetail> GetOrderDetails()
    {
        return Items.Select(i => new OrderDetail
        {
            ProductId = i.Product.Id,
            Quantity = i.ConvertedQuantity,
            UnitPrice = i.UnitPriceConverted,
            SubTotal = i.SubTotal
        }).ToList();
    }
}

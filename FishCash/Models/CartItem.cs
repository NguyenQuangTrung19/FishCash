using CommunityToolkit.Mvvm.ComponentModel;

namespace FishCash.Models;

/// <summary>
/// Represents an item in the shopping cart with observable quantity for UI binding
/// </summary>
public partial class CartItem : ObservableObject
{
    public Product Product { get; set; } = null!;

    [ObservableProperty]
    private decimal quantity;

    public decimal SubTotal => Quantity * Product.Price;

    partial void OnQuantityChanged(decimal value)
    {
        OnPropertyChanged(nameof(SubTotal));
    }
}

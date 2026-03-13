using CommunityToolkit.Mvvm.ComponentModel;

namespace FishCash.Models;

/// <summary>
/// Represents an item in the shopping cart with unit conversion support.
/// Stores both the entered quantity/unit and the converted quantity in display unit.
/// </summary>
public partial class CartItem : ObservableObject
{
    public Product Product { get; set; } = null!;

    /// <summary>
    /// Quantity as entered by user (e.g. 4.2)
    /// </summary>
    [ObservableProperty]
    private decimal enteredQuantity;

    /// <summary>
    /// Unit as entered by user (e.g. "tấn")
    /// </summary>
    public string EnteredUnit { get; set; } = "kg";

    /// <summary>
    /// The common display unit selected in POS (e.g. "kg")
    /// </summary>
    public string DisplayUnit { get; set; } = "kg";

    /// <summary>
    /// Quantity converted to the display unit.
    /// E.g. 4.2 tấn → 4200 kg (if display unit is kg)
    /// </summary>
    public decimal ConvertedQuantity => ConvertToDisplayUnit(EnteredQuantity, EnteredUnit, DisplayUnit);

    /// <summary>
    /// Product price converted to per-display-unit.
    /// E.g. 100,000,000 đ/tấn → 100,000 đ/kg (if display unit is kg)
    /// </summary>
    public decimal UnitPriceConverted => ConvertPrice(Product.Price, Product.Unit, DisplayUnit);

    /// <summary>
    /// Total = ConvertedQuantity × UnitPriceConverted
    /// </summary>
    public decimal SubTotal => ConvertedQuantity * UnitPriceConverted;

    // For backward compatibility - Quantity maps to EnteredQuantity
    public decimal Quantity
    {
        get => EnteredQuantity;
        set => EnteredQuantity = value;
    }

    partial void OnEnteredQuantityChanged(decimal value)
    {
        OnPropertyChanged(nameof(Quantity));
        OnPropertyChanged(nameof(ConvertedQuantity));
        OnPropertyChanged(nameof(SubTotal));
    }

    /// <summary>
    /// Call when display unit changes to refresh computed properties
    /// </summary>
    public void RefreshConversions()
    {
        OnPropertyChanged(nameof(ConvertedQuantity));
        OnPropertyChanged(nameof(UnitPriceConverted));
        OnPropertyChanged(nameof(SubTotal));
    }

    /// <summary>
    /// Convert a quantity from sourceUnit to targetUnit.
    /// Both units are first converted to kg, then to target.
    /// </summary>
    private static decimal ConvertToDisplayUnit(decimal qty, string fromUnit, string toUnit)
    {
        decimal qtyInKg = ToKg(qty, fromUnit);
        return FromKg(qtyInKg, toUnit);
    }

    /// <summary>
    /// Convert a price-per-unit from sourceUnit to targetUnit.
    /// If price is per-tấn and target is kg: price / 1000
    /// If price is per-kg and target is tấn: price * 1000
    /// </summary>
    private static decimal ConvertPrice(decimal pricePerUnit, string fromUnit, string toUnit)
    {
        if (string.Equals(fromUnit, toUnit, StringComparison.OrdinalIgnoreCase))
            return pricePerUnit;

        // price per fromUnit → price per kg → price per toUnit
        // 1 fromUnit = ToKg(1, fromUnit) kg
        // price per 1 fromUnit = pricePerUnit
        // price per 1 kg = pricePerUnit / ToKg(1, fromUnit)
        decimal kgPerFromUnit = ToKg(1, fromUnit);
        decimal pricePerKg = pricePerUnit / kgPerFromUnit;
        decimal kgPerToUnit = ToKg(1, toUnit);
        return pricePerKg * kgPerToUnit;
    }

    private static decimal ToKg(decimal qty, string unit)
    {
        return unit.ToLowerInvariant() switch
        {
            "tấn" => qty * 1000m,
            "kg" => qty,
            _ => qty
        };
    }

    private static decimal FromKg(decimal kgQty, string unit)
    {
        return unit.ToLowerInvariant() switch
        {
            "tấn" => kgQty / 1000m,
            "kg" => kgQty,
            _ => kgQty
        };
    }
}

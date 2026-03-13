using System.ComponentModel.DataAnnotations;

namespace FishCash.Models;

/// <summary>
/// Represents a trading partner - either a supplier (chủ ghe) or buyer (nhà hàng, cơ sở)
/// </summary>
public class Partner
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên đối tác không được để trống")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public PartnerType PartnerType { get; set; } = PartnerType.Supplier;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Note { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<TradeOrder> TradeOrders { get; set; } = new List<TradeOrder>();
}

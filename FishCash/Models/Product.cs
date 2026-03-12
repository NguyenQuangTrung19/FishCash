using System.ComponentModel.DataAnnotations;

namespace FishCash.Models;

/// <summary>
/// Represents a seafood product in the store inventory
/// </summary>
public class Product
{
    public int Id { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
    [MaxLength(200, ErrorMessage = "Tên sản phẩm tối đa 200 ký tự")]
    public string Name { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
    public decimal Price { get; set; }

    [MaxLength(50)]
    public string Unit { get; set; } = "kg"; // kg, con, gram, khay...

    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

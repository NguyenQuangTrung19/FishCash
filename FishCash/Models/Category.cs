using System.ComponentModel.DataAnnotations;

namespace FishCash.Models;

/// <summary>
/// Represents a product category (e.g., Cá, Tôm, Mực)
/// </summary>
public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên danh mục không được để trống")]
    [MaxLength(100, ErrorMessage = "Tên danh mục tối đa 100 ký tự")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
    public string Description { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

using FishCash.Models;

namespace FishCash.Services;

/// <summary>
/// Interface for product management operations
/// </summary>
public interface IProductService
{
    Task<List<Product>> GetProductsAsync();
    Task<List<Product>> GetProductsByCategoryAsync(int categoryId);
    Task AddProductAsync(Product product);
    Task UpdateProductAsync(Product product);
    Task DeleteProductAsync(Product product);
}

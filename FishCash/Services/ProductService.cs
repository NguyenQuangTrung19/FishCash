using FishCash.Data;
using FishCash.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FishCash.Services;

/// <summary>
/// Service for managing seafood products
/// </summary>
public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(AppDbContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        try
        {
            return await _context.Products.Include(p => p.Category).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading products");
            throw;
        }
    }
    
    public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
    {
        try
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading products by category: {CategoryId}", categoryId);
            throw;
        }
    }

    public async Task AddProductAsync(Product product)
    {
        try
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Added product: {Name} (Price: {Price})", product.Name, product.Price);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product: {Name}", product.Name);
            throw;
        }
    }

    public async Task UpdateProductAsync(Product product)
    {
        try
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated product: {Id} - {Name}", product.Id, product.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product: {Id}", product.Id);
            throw;
        }
    }

    public async Task DeleteProductAsync(Product product)
    {
        try
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted product: {Id} - {Name}", product.Id, product.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product: {Id}", product.Id);
            throw;
        }
    }
}

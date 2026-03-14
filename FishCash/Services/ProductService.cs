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
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IDbContextFactory<AppDbContext> contextFactory, ILogger<ProductService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Products.Include(p => p.Category).ToListAsync();
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
            using var context = _contextFactory.CreateDbContext();
            return await context.Products
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
            using var context = _contextFactory.CreateDbContext();
            context.Products.Add(product);
            await context.SaveChangesAsync();
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
            using var context = _contextFactory.CreateDbContext();
            context.Products.Update(product);
            await context.SaveChangesAsync();
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
            using var context = _contextFactory.CreateDbContext();
            context.Products.Remove(product);
            await context.SaveChangesAsync();
            _logger.LogInformation("Deleted product: {Id} - {Name}", product.Id, product.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product: {Id}", product.Id);
            throw;
        }
    }
}

using FishCash.Data;
using FishCash.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FishCash.Services;

/// <summary>
/// Service for managing product categories
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(IDbContextFactory<AppDbContext> contextFactory, ILogger<CategoryService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Categories.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categories");
            throw;
        }
    }

    public async Task AddCategoryAsync(Category category)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            context.Categories.Add(category);
            await context.SaveChangesAsync();
            _logger.LogInformation("Added category: {Name}", category.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding category: {Name}", category.Name);
            throw;
        }
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            context.Categories.Update(category);
            await context.SaveChangesAsync();
            _logger.LogInformation("Updated category: {Id} - {Name}", category.Id, category.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category: {Id}", category.Id);
            throw;
        }
    }

    public async Task DeleteCategoryAsync(Category category)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            context.Categories.Remove(category);
            await context.SaveChangesAsync();
            _logger.LogInformation("Deleted category: {Id} - {Name}", category.Id, category.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category: {Id}", category.Id);
            throw;
        }
    }
}

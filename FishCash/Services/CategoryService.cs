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
    private readonly AppDbContext _context;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(AppDbContext context, ILogger<CategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        try
        {
            return await _context.Categories.ToListAsync();
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
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
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
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
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
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted category: {Id} - {Name}", category.Id, category.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category: {Id}", category.Id);
            throw;
        }
    }
}

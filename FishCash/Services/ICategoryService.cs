using FishCash.Models;

namespace FishCash.Services;

/// <summary>
/// Interface for category management operations
/// </summary>
public interface ICategoryService
{
    Task<List<Category>> GetCategoriesAsync();
    Task AddCategoryAsync(Category category);
    Task UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(Category category);
}

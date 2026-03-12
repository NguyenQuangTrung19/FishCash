using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishCash.Models;
using FishCash.Services;

namespace FishCash.ViewModels;

public partial class CategoryViewModel : BaseViewModel
{
    private readonly ICategoryService _categoryService;

    public ObservableCollection<Category> Categories { get; } = new();

    [ObservableProperty]
    private string newCategoryName = string.Empty;

    public CategoryViewModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;
        Title = "Quản lý Danh mục";
    }

    [RelayCommand]
    public async Task LoadCategoriesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var categories = await _categoryService.GetCategoriesAsync();
            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể tải danh mục: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task AddCategoryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCategoryName))
        {
            await Shell.Current.DisplayAlert("Cảnh báo", "Tên danh mục không được để trống", "OK");
            return;
        }

        try
        {
            var category = new Category { Name = NewCategoryName.Trim() };
            await _categoryService.AddCategoryAsync(category);
            Categories.Add(category);
            NewCategoryName = string.Empty;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể thêm danh mục: {ex.Message}", "OK");
        }
    }
}

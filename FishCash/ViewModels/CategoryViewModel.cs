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

    [ObservableProperty]
    private string newCategoryDescription = string.Empty;

    [ObservableProperty]
    private bool isFormVisible;

    [ObservableProperty]
    private Category? editingCategory;

    /// <summary>
    /// True when editing an existing category
    /// </summary>
    public bool IsEditing => EditingCategory != null;

    public CategoryViewModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;
        Title = "Quản lý Danh mục";
    }

    [RelayCommand]
    public void ToggleForm()
    {
        if (IsFormVisible)
        {
            IsFormVisible = false;
            ResetForm();
        }
        else
        {
            IsFormVisible = true;
        }
    }

    private void ResetForm()
    {
        EditingCategory = null;
        NewCategoryName = string.Empty;
        NewCategoryDescription = string.Empty;
        OnPropertyChanged(nameof(IsEditing));
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
            foreach (var c in categories) Categories.Add(c);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể tải danh mục: {ex.Message}", "OK");
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public async Task AddCategoryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCategoryName))
        {
            await Shell.Current.DisplayAlert("Cảnh báo", "Tên danh mục không được để trống.", "OK");
            return;
        }

        try
        {
            if (IsEditing && EditingCategory != null)
            {
                // Update existing
                EditingCategory.Name = NewCategoryName.Trim();
                EditingCategory.Description = NewCategoryDescription.Trim();
                await _categoryService.UpdateCategoryAsync(EditingCategory);
                await LoadCategoriesAsync();
            }
            else
            {
                // Add new
                var category = new Category
                {
                    Name = NewCategoryName.Trim(),
                    Description = NewCategoryDescription.Trim()
                };
                await _categoryService.AddCategoryAsync(category);
                Categories.Add(category);
            }

            IsFormVisible = false;
            ResetForm();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể lưu danh mục: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public void EditCategory(Category category)
    {
        if (category == null) return;
        EditingCategory = category;
        NewCategoryName = category.Name;
        NewCategoryDescription = category.Description;
        IsFormVisible = true;
        OnPropertyChanged(nameof(IsEditing));
    }

    [RelayCommand]
    public async Task DeleteCategoryAsync(Category category)
    {
        if (category == null) return;

        var confirm = await Shell.Current.DisplayAlert(
            "Xác nhận xóa",
            $"Bạn có chắc muốn xóa danh mục \"{category.Name}\"?\nCác sản phẩm thuộc danh mục này cũng sẽ bị ảnh hưởng.",
            "Xóa", "Hủy");
        if (!confirm) return;

        try
        {
            await _categoryService.DeleteCategoryAsync(category);
            Categories.Remove(category);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể xóa: {ex.Message}", "OK");
        }
    }
}

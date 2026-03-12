using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishCash.Models;
using FishCash.Services;

namespace FishCash.ViewModels;

public partial class ProductViewModel : BaseViewModel
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();

    [ObservableProperty]
    private string newProductName = string.Empty;

    [ObservableProperty]
    private decimal newProductPrice;

    [ObservableProperty]
    private string newProductUnit = "kg";
    
    [ObservableProperty]
    private Category? selectedCategory;

    public ProductViewModel(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
        Title = "Quản lý Sản phẩm";
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            
            // Load Categories
            var categories = await _categoryService.GetCategoriesAsync();
            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }

            // Load Products
            var products = await _productService.GetProductsAsync();
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể tải dữ liệu: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task AddProductAsync()
    {
        if (string.IsNullOrWhiteSpace(NewProductName) || SelectedCategory == null || NewProductPrice <= 0)
        {
            await Shell.Current.DisplayAlert("Lỗi", "Vui lòng điền đủ Tên, Giá và chọn Danh mục hợp lệ.", "OK");
            return;
        }

        try
        {
            var product = new Product 
            { 
                Name = NewProductName.Trim(),
                Price = NewProductPrice,
                Unit = NewProductUnit.Trim(),
                CategoryId = SelectedCategory.Id,
                IsActive = true
            };
            
            await _productService.AddProductAsync(product);
            
            // Refresh
            product.Category = SelectedCategory;
            Products.Add(product);
            
            // Reset fields
            NewProductName = string.Empty;
            NewProductPrice = 0;
            // Keep Unit and SelectedCategory for convenience
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể thêm sản phẩm: {ex.Message}", "OK");
        }
    }
}

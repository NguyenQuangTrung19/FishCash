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
    public ObservableCollection<string> UnitOptions { get; } = new() { "kg", "tấn" };

    [ObservableProperty]
    private string newProductName = string.Empty;

    [ObservableProperty]
    private string newProductPriceText = string.Empty;

    [ObservableProperty]
    private string newProductUnit = "kg";
    
    [ObservableProperty]
    private Category? selectedCategory;

    [ObservableProperty]
    private bool isFormVisible;

    [ObservableProperty]
    private Product? editingProduct;

    // Flag to prevent re-entrant formatting
    private bool _isFormatting;

    /// <summary>
    /// True when editing an existing product, false when adding new
    /// </summary>
    public bool IsEditing => EditingProduct != null;

    public ProductViewModel(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
        Title = "Quản lý Sản phẩm";
    }

    /// <summary>
    /// Auto-format price text when user types (e.g. 100000 → 100.000)
    /// </summary>
    partial void OnNewProductPriceTextChanged(string value)
    {
        if (_isFormatting) return;

        var digitsOnly = new string(value.Where(char.IsDigit).ToArray());
        if (string.IsNullOrEmpty(digitsOnly)) return;

        if (decimal.TryParse(digitsOnly, out decimal number))
        {
            var formatted = number.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
            if (formatted != value)
            {
                _isFormatting = true;
                NewProductPriceText = formatted;
                _isFormatting = false;
            }
        }
    }

    [RelayCommand]
    public void ToggleForm()
    {
        if (IsFormVisible)
        {
            // Closing form - reset editing state
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
        EditingProduct = null;
        NewProductName = string.Empty;
        _isFormatting = true;
        NewProductPriceText = string.Empty;
        _isFormatting = false;
        NewProductUnit = "kg";
        SelectedCategory = null;
        OnPropertyChanged(nameof(IsEditing));
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            var categories = await _categoryService.GetCategoriesAsync();
            Categories.Clear();
            foreach (var c in categories) Categories.Add(c);

            var products = await _productService.GetProductsAsync();
            Products.Clear();
            foreach (var p in products) Products.Add(p);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể tải dữ liệu: {ex.Message}", "OK");
        }
        finally { IsBusy = false; }
    }

    private bool TryParsePrice(string priceText, out decimal price)
    {
        price = 0;
        if (string.IsNullOrWhiteSpace(priceText)) return false;
        var cleaned = priceText.Replace(".", "").Replace(",", "").Trim();
        return decimal.TryParse(cleaned, out price) && price > 0;
    }

    [RelayCommand]
    public async Task AddProductAsync()
    {
        if (string.IsNullOrWhiteSpace(NewProductName) || SelectedCategory == null)
        {
            await Shell.Current.DisplayAlert("Lỗi", "Vui lòng điền đủ Tên và chọn Danh mục.", "OK");
            return;
        }
        if (!TryParsePrice(NewProductPriceText, out decimal price))
        {
            await Shell.Current.DisplayAlert("Lỗi", "Giá bán không hợp lệ.", "OK");
            return;
        }

        try
        {
            if (IsEditing && EditingProduct != null)
            {
                // Update existing product
                EditingProduct.Name = NewProductName.Trim();
                EditingProduct.Price = price;
                EditingProduct.Unit = NewProductUnit.Trim();
                EditingProduct.CategoryId = SelectedCategory.Id;
                EditingProduct.Category = SelectedCategory;

                await _productService.UpdateProductAsync(EditingProduct);
                // Refresh list to show changes
                await LoadDataAsync();
            }
            else
            {
                // Add new product
                var product = new Product
                {
                    Name = NewProductName.Trim(),
                    Price = price,
                    Unit = NewProductUnit.Trim(),
                    CategoryId = SelectedCategory.Id,
                    IsActive = true
                };
                await _productService.AddProductAsync(product);
                product.Category = SelectedCategory;
                Products.Add(product);
            }

            IsFormVisible = false;
            ResetForm();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể lưu sản phẩm: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public void EditProduct(Product product)
    {
        if (product == null) return;

        EditingProduct = product;
        NewProductName = product.Name;
        _isFormatting = true;
        NewProductPriceText = product.Price.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
        _isFormatting = false;
        NewProductUnit = product.Unit;
        SelectedCategory = Categories.FirstOrDefault(c => c.Id == product.CategoryId);
        IsFormVisible = true;
        OnPropertyChanged(nameof(IsEditing));
    }

    [RelayCommand]
    public async Task DeleteProductAsync(Product product)
    {
        if (product == null) return;

        var confirm = await Shell.Current.DisplayAlert(
            "Xác nhận xóa",
            $"Bạn có chắc muốn xóa sản phẩm \"{product.Name}\"?",
            "Xóa", "Hủy");
        if (!confirm) return;

        try
        {
            await _productService.DeleteProductAsync(product);
            Products.Remove(product);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể xóa: {ex.Message}", "OK");
        }
    }
}

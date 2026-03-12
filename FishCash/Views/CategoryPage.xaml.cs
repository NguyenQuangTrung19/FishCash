using FishCash.ViewModels;

namespace FishCash.Views;

public partial class CategoryPage : ContentPage
{
    private readonly CategoryViewModel _viewModel;

    public CategoryPage(CategoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCategoriesAsync();
    }
}

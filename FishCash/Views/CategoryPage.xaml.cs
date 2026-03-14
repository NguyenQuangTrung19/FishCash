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
        try
        {
            // Reset IsBusy in case it was left stuck from a previous interrupted load
            _viewModel.IsBusy = false;
            await _viewModel.LoadCategoriesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CategoryPage] OnAppearing error: {ex.Message}");
        }
    }
}

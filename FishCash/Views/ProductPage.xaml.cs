using FishCash.ViewModels;

namespace FishCash.Views;

public partial class ProductPage : ContentPage
{
    private readonly ProductViewModel _viewModel;

    public ProductPage(ProductViewModel viewModel)
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
            await _viewModel.LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductPage] OnAppearing error: {ex.Message}");
        }
    }
}

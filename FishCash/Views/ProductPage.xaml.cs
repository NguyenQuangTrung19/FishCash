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
        await _viewModel.LoadDataAsync();
    }
}

using FishCash.ViewModels;

namespace FishCash.Views;

public partial class PosPage : ContentPage
{
    private readonly PosViewModel _viewModel;

    public PosPage(PosViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadProductsAsync();
    }
}

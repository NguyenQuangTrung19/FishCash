using FishCash.ViewModels;

namespace FishCash.Views;

public partial class TradingSessionPage : ContentPage
{
    private readonly TradingSessionViewModel _viewModel;

    public TradingSessionPage(TradingSessionViewModel viewModel)
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
            await _viewModel.LoadSessionsCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TradingSessionPage] OnAppearing error: {ex.Message}");
        }
    }
}

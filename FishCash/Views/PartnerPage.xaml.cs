using FishCash.ViewModels;

namespace FishCash.Views;

public partial class PartnerPage : ContentPage
{
    private readonly PartnerViewModel _viewModel;

    public PartnerPage(PartnerViewModel viewModel)
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
            await _viewModel.LoadPartnersCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PartnerPage] OnAppearing error: {ex.Message}");
        }
    }
}

using FishCash.ViewModels;

namespace FishCash.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;

    public DashboardPage(DashboardViewModel viewModel)
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
            _viewModel.IsBusy = false;
            await _viewModel.LoadDashboardDataCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardPage] OnAppearing error: {ex.Message}");
        }
    }

    // Chart hover/tap interaction
    private void ChartView_StartHoverInteraction(object sender, TouchEventArgs e)
    {
        if (e.Touches?.Length > 0)
            _viewModel.ChartInteractionCommand.Execute(e.Touches[0]);
    }

    private void ChartView_MoveHoverInteraction(object sender, TouchEventArgs e)
    {
        if (e.Touches?.Length > 0)
            _viewModel.ChartInteractionCommand.Execute(e.Touches[0]);
    }

    private void ChartView_EndHoverInteraction(object sender, EventArgs e)
    {
        if (_viewModel.ChartDrawable != null)
        {
            _viewModel.ChartDrawable.HoveredIndex = null;
            _viewModel.IsChartTooltipVisible = false;
            OnPropertyChanged(nameof(_viewModel.ChartDrawable));
            (sender as GraphicsView)?.Invalidate();
        }
    }

    private void ChartView_StartInteraction(object sender, TouchEventArgs e)
    {
        if (e.Touches?.Length > 0)
            _viewModel.ChartInteractionCommand.Execute(e.Touches[0]);
    }
}

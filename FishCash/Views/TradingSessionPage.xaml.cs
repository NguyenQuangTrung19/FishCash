using FishCash.ViewModels;

namespace FishCash.Views;

public partial class TradingSessionPage : ContentPage
{
    public TradingSessionPage(TradingSessionViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TradingSessionViewModel vm)
            vm.LoadSessionsCommand.Execute(null);
    }
}

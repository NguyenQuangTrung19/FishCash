using FishCash.ViewModels;

namespace FishCash.Views;

public partial class PartnerPage : ContentPage
{
    public PartnerPage(PartnerViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PartnerViewModel vm)
            vm.LoadPartnersCommand.Execute(null);
    }
}

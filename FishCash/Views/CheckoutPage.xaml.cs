using FishCash.ViewModels;

namespace FishCash.Views;

public partial class CheckoutPage : ContentPage
{
    public CheckoutPage(CheckoutViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

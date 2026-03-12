namespace FishCash;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(FishCash.Views.CheckoutPage), typeof(FishCash.Views.CheckoutPage));
	}
}

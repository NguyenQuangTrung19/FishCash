using Microsoft.Extensions.Logging;

namespace FishCash;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Database
		builder.Services.AddDbContext<FishCash.Data.AppDbContext>();

		// Services
		builder.Services.AddTransient<FishCash.Services.ICategoryService, FishCash.Services.CategoryService>();
		builder.Services.AddTransient<FishCash.Services.IProductService, FishCash.Services.ProductService>();
		builder.Services.AddTransient<FishCash.Services.IOrderService, FishCash.Services.OrderService>();
		builder.Services.AddTransient<FishCash.Services.IPartnerService, FishCash.Services.PartnerService>();
		builder.Services.AddTransient<FishCash.Services.ITradingService, FishCash.Services.TradingService>();
		builder.Services.AddSingleton<FishCash.Services.CartService>();
		builder.Services.AddSingleton<FishCash.Services.PrintService>();

		// ViewModels
		builder.Services.AddTransient<FishCash.ViewModels.DashboardViewModel>();
		builder.Services.AddTransient<FishCash.ViewModels.CategoryViewModel>();
		builder.Services.AddTransient<FishCash.ViewModels.ProductViewModel>();
		builder.Services.AddTransient<FishCash.ViewModels.PosViewModel>();
		builder.Services.AddTransient<FishCash.ViewModels.CheckoutViewModel>();
		builder.Services.AddTransient<FishCash.ViewModels.PartnerViewModel>();
		builder.Services.AddTransient<FishCash.ViewModels.TradingSessionViewModel>();

		// Views
		builder.Services.AddTransient<FishCash.Views.DashboardPage>();
		builder.Services.AddTransient<FishCash.Views.CategoryPage>();
		builder.Services.AddTransient<FishCash.Views.ProductPage>();
		builder.Services.AddTransient<FishCash.Views.PosPage>();
		builder.Services.AddTransient<FishCash.Views.CheckoutPage>();
		builder.Services.AddTransient<FishCash.Views.PartnerPage>();
		builder.Services.AddTransient<FishCash.Views.TradingSessionPage>();

		var app = builder.Build();

		// Initialize database once at startup
		using (var scope = app.Services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<FishCash.Data.AppDbContext>();
			db.Database.EnsureCreated();
		}

		return app;
	}
}

using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using CommunityToolkit.Maui;
using QuestPDF.Infrastructure;

namespace FishCash;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		// QuestPDF Community License (free for revenue < $1M)
		QuestPDF.Settings.License = LicenseType.Community;

		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Database - Use DbContextFactory to avoid lifecycle issues in MAUI
		// Each service creates its own short-lived DbContext per operation
		string dbPath = Path.Combine(FileSystem.AppDataDirectory, FishCash.Helpers.AppConstants.DatabaseName);
		builder.Services.AddDbContextFactory<FishCash.Data.AppDbContext>(options =>
			options.UseSqlite($"Filename={dbPath}"));

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

		// Initialize database - create missing tables for new entities
		try
		{
			var factory = app.Services.GetRequiredService<IDbContextFactory<FishCash.Data.AppDbContext>>();
			using (var db = factory.CreateDbContext())
			{
				db.Database.EnsureCreated();

				// EnsureCreated does NOT add new tables to an existing DB.
				// Manually create missing tables via raw SQL.
				db.Database.ExecuteSqlRaw(@"
					CREATE TABLE IF NOT EXISTS Partners (
						Id INTEGER PRIMARY KEY AUTOINCREMENT,
						Name TEXT NOT NULL DEFAULT '',
						PartnerType TEXT NOT NULL DEFAULT 'Supplier',
						Phone TEXT NOT NULL DEFAULT '',
						Address TEXT NOT NULL DEFAULT '',
						Note TEXT NOT NULL DEFAULT '',
						IsActive INTEGER NOT NULL DEFAULT 1
					)");

				db.Database.ExecuteSqlRaw(@"
					CREATE TABLE IF NOT EXISTS TradingSessions (
						Id INTEGER PRIMARY KEY AUTOINCREMENT,
						SessionDate TEXT NOT NULL DEFAULT '',
						Note TEXT NOT NULL DEFAULT '',
						TotalPurchase TEXT NOT NULL DEFAULT '0',
						TotalSales TEXT NOT NULL DEFAULT '0',
						Status TEXT NOT NULL DEFAULT 'Active'
					)");

				db.Database.ExecuteSqlRaw(@"
					CREATE TABLE IF NOT EXISTS TradeOrders (
						Id INTEGER PRIMARY KEY AUTOINCREMENT,
						TradingSessionId INTEGER NOT NULL,
						PartnerId INTEGER NULL,
						PartnerName TEXT NOT NULL DEFAULT '',
						OrderType TEXT NOT NULL DEFAULT 'Purchase',
						OrderDate TEXT NOT NULL DEFAULT '',
						TotalAmount TEXT NOT NULL DEFAULT '0',
						Note TEXT NOT NULL DEFAULT '',
						FOREIGN KEY (TradingSessionId) REFERENCES TradingSessions(Id),
						FOREIGN KEY (PartnerId) REFERENCES Partners(Id)
					)");


				db.Database.ExecuteSqlRaw(@"
					CREATE TABLE IF NOT EXISTS TradeOrderDetails (
						Id INTEGER PRIMARY KEY AUTOINCREMENT,
						TradeOrderId INTEGER NOT NULL,
						ProductId INTEGER NOT NULL,
						Quantity TEXT NOT NULL DEFAULT '0',
						Unit TEXT NOT NULL DEFAULT 'kg',
						UnitPrice TEXT NOT NULL DEFAULT '0',
						SubTotal TEXT NOT NULL DEFAULT '0',
						FOREIGN KEY (TradeOrderId) REFERENCES TradeOrders(Id),
						FOREIGN KEY (ProductId) REFERENCES Products(Id)
					)");
				// Migration: fix PartnerId NOT NULL constraint → should be nullable
				// SQLite doesn't support ALTER COLUMN, so recreate the table
				try
				{
					// Check if PartnerId is NOT NULL (incorrect)
					var tableInfo = new List<(string name, bool notNull)>();
					using (var cmd = db.Database.GetDbConnection().CreateCommand())
					{
						db.Database.OpenConnection();
						cmd.CommandText = "PRAGMA table_info(TradeOrders)";
						using var reader = cmd.ExecuteReader();
						while (reader.Read())
						{
							tableInfo.Add((reader.GetString(1), reader.GetBoolean(3)));
						}
					}

					var partnerCol = tableInfo.FirstOrDefault(c => c.name == "PartnerId");
					if (partnerCol != default && partnerCol.notNull)
					{
						// PartnerId is NOT NULL — needs fix via table recreation
						db.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF");
						db.Database.ExecuteSqlRaw(@"
							CREATE TABLE TradeOrders_new (
								Id INTEGER PRIMARY KEY AUTOINCREMENT,
								TradingSessionId INTEGER NOT NULL,
								PartnerId INTEGER NULL,
								PartnerName TEXT NOT NULL DEFAULT '',
								OrderType TEXT NOT NULL DEFAULT 'Purchase',
								OrderDate TEXT NOT NULL DEFAULT '',
								TotalAmount TEXT NOT NULL DEFAULT '0',
								Note TEXT NOT NULL DEFAULT '',
								FOREIGN KEY (TradingSessionId) REFERENCES TradingSessions(Id),
								FOREIGN KEY (PartnerId) REFERENCES Partners(Id)
							)");
						db.Database.ExecuteSqlRaw(
							"INSERT INTO TradeOrders_new SELECT * FROM TradeOrders");
						db.Database.ExecuteSqlRaw("DROP TABLE TradeOrders");
						db.Database.ExecuteSqlRaw(
							"ALTER TABLE TradeOrders_new RENAME TO TradeOrders");
						db.Database.ExecuteSqlRaw("PRAGMA foreign_keys = ON");
						System.Diagnostics.Debug.WriteLine("MIGRATION: Fixed TradeOrders.PartnerId to nullable");
					}
				}
				catch (Exception migEx)
				{
					System.Diagnostics.Debug.WriteLine($"Migration warning: {migEx.Message}");
				}

				// Migration: add PartnerName column if missing (for existing databases)
				try
				{
					db.Database.ExecuteSqlRaw(
						"ALTER TABLE TradeOrders ADD COLUMN PartnerName TEXT NOT NULL DEFAULT ''");
				}
				catch { /* Column already exists */ }
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"CRITICAL DB ERROR: {ex.Message}");
		}

		return app;
	}
}

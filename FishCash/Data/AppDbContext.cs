using Microsoft.EntityFrameworkCore;
using FishCash.Models;
using FishCash.Helpers;

namespace FishCash.Data;

/// <summary>
/// EF Core database context for FishCash application
/// </summary>
public class AppDbContext : DbContext
{
    // Original tables
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    // Broker trading tables
    public DbSet<Partner> Partners { get; set; }
    public DbSet<TradingSession> TradingSessions { get; set; }
    public DbSet<TradeOrder> TradeOrders { get; set; }
    public DbSet<TradeOrderDetail> TradeOrderDetails { get; set; }

    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, AppConstants.DatabaseName);
            optionsBuilder.UseSqlite($"Filename={dbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ═══ Original relationships ═══
        modelBuilder.Entity<Category>()
            .HasMany(c => c.Products)
            .WithOne(p => p.Category)
            .HasForeignKey(p => p.CategoryId);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.OrderDetails)
            .WithOne(od => od.Order)
            .HasForeignKey(od => od.OrderId);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Transaction)
            .WithOne(t => t.Order)
            .HasForeignKey<Transaction>(t => t.OrderId);

        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Product)
            .WithMany()
            .HasForeignKey(od => od.ProductId);

        // ═══ Broker trading relationships ═══
        modelBuilder.Entity<TradingSession>()
            .HasMany(ts => ts.TradeOrders)
            .WithOne(to => to.TradingSession)
            .HasForeignKey(to => to.TradingSessionId);

        // Ignore computed property (not a database column)
        modelBuilder.Entity<TradingSession>()
            .Ignore(ts => ts.Profit);

        modelBuilder.Entity<Partner>()
            .HasMany(p => p.TradeOrders)
            .WithOne(to => to.Partner)
            .HasForeignKey(to => to.PartnerId)
            .IsRequired(false);

        // Ignore computed property
        modelBuilder.Entity<TradeOrder>()
            .Ignore(to => to.DisplayPartnerName);

        modelBuilder.Entity<TradeOrder>()
            .HasMany(to => to.Details)
            .WithOne(d => d.TradeOrder)
            .HasForeignKey(d => d.TradeOrderId);

        modelBuilder.Entity<TradeOrderDetail>()
            .HasOne(d => d.Product)
            .WithMany()
            .HasForeignKey(d => d.ProductId);

        // ═══ Enum conversions ═══
        modelBuilder.Entity<Order>()
            .Property(o => o.PaymentMethod)
            .HasConversion<string>();

        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Transaction>()
            .Property(t => t.TransactionType)
            .HasConversion<string>();

        modelBuilder.Entity<Partner>()
            .Property(p => p.PartnerType)
            .HasConversion<string>();

        modelBuilder.Entity<TradeOrder>()
            .Property(o => o.OrderType)
            .HasConversion<string>();

        // ═══ Decimal precision ═══
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.Property(od => od.Quantity).HasPrecision(18, 3);
            entity.Property(od => od.UnitPrice).HasPrecision(18, 2);
            entity.Property(od => od.SubTotal).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<TradingSession>(entity =>
        {
            entity.Property(s => s.TotalPurchase).HasPrecision(18, 2);
            entity.Property(s => s.TotalSales).HasPrecision(18, 2);
        });

        modelBuilder.Entity<TradeOrder>()
            .Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<TradeOrderDetail>(entity =>
        {
            entity.Property(d => d.Quantity).HasPrecision(18, 3);
            entity.Property(d => d.UnitPrice).HasPrecision(18, 2);
            entity.Property(d => d.SubTotal).HasPrecision(18, 2);
        });
    }
}

using Microsoft.EntityFrameworkCore;
using FishCash.Models;
using FishCash.Helpers;

namespace FishCash.Data;

/// <summary>
/// EF Core database context for FishCash application
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    public AppDbContext()
    {
        // For EF Core Tools if needed, otherwise initialized via Dependency Injection
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

        // Relationships
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

        // Enum conversions - store as string for backward compatibility
        modelBuilder.Entity<Order>()
            .Property(o => o.PaymentMethod)
            .HasConversion<string>();

        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Transaction>()
            .Property(t => t.TransactionType)
            .HasConversion<string>();

        // Decimal precision for currency fields
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
    }
}

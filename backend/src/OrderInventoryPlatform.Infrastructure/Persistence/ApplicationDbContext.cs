using Microsoft.EntityFrameworkCore;
using OrderInventoryPlatform.Domain.Catalog;
using OrderInventoryPlatform.Domain.Inventory;
using OrderInventoryPlatform.Domain.Orders;

namespace OrderInventoryPlatform.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.Sku).IsUnique();
            entity.Property(p => p.Sku).IsRequired().HasMaxLength(Product.SkuMaxLength);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(Product.NameMaxLength);
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.ReorderThreshold);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.CreatedAtUtc).IsRequired();
            entity.Property(o => o.Status).HasConversion<int>();
            entity.Ignore(o => o.Lines);

            entity.HasMany<OrderLine>("_lines")
                .WithOne()
                .HasForeignKey(l => l.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Navigation("_lines").UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<OrderLine>(entity =>
        {
            entity.ToTable("OrderLines");
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Quantity).IsRequired();
            entity.Property(l => l.UnitPrice).HasPrecision(18, 2);
            entity.Property(l => l.LineTotal).HasPrecision(18, 2);
            entity.HasIndex(l => l.OrderId);
            entity.HasIndex(l => l.ProductId);
        });

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.ToTable("InventoryItems");
            entity.HasKey(i => i.Id);
            entity.HasIndex(i => i.ProductId).IsUnique();
            entity.Property(i => i.AvailableQuantity).IsRequired();
        });

        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.ToTable("InventoryMovements");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Type).HasConversion<int>();
            entity.Property(m => m.CreatedAtUtc).IsRequired();
            entity.Property(m => m.Reason)
                .IsRequired()
                .HasMaxLength(InventoryMovement.ReasonMaxLength);
            entity.HasIndex(m => m.ProductId);
            entity.HasIndex(m => m.RelatedOrderId);
        });
    }
}

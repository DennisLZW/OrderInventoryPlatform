using Microsoft.EntityFrameworkCore;
using OrderInventoryPlatform.Domain.Catalog;
using OrderInventoryPlatform.Domain.Inventory;

namespace OrderInventoryPlatform.Infrastructure.Persistence;

/// <summary>
/// Development-only catalog + inventory seed. Idempotent: no-op if any product already exists.
/// </summary>
public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        // Realistic warehouse / e-commerce style samples (USD-style pricing).
        var productsWithStock = new (Product Product, int AvailableQuantity)[]
        {
            (Product.Create("WH-TS-4X6-01", "Direct Thermal Labels 4\" × 6\" (Roll of 500)", 42.99m, 24), 180),
            (Product.Create("PKG-CBOX-M-25", "Corrugated Mailer Box — Medium (25 pack)", 38.50m, 15), 96),
            (Product.Create("SAFE-NIT-L-100", "Industrial Nitrile Gloves — Large (box of 100)", 22.75m, 40), 220),
            (Product.Create("BEV-VAC-750", "Insulated Stainless Bottle 750ml — Matte Black", 34.00m, 20), 64),
            (Product.Create("TAP-HD-6PK", "Heavy-Duty Packaging Tape — 48mm × 50m (6-pack)", 16.49m, 50), 400),
            (Product.Create("ORG-SHELF-5T", "Wire Shelving Unit 5-Tier — 48\"W Chrome", 189.00m, 6), 12)
        };

        foreach (var (product, _) in productsWithStock)
        {
            db.Products.Add(product);
        }

        foreach (var (product, available) in productsWithStock)
        {
            db.InventoryItems.Add(InventoryItem.CreateForProduct(product.Id, available));
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}

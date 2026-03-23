using OrderInventoryPlatform.Domain.Common;

namespace OrderInventoryPlatform.Domain.Catalog;

/// <summary>
/// Catalog aggregate root. Stock levels are not stored here — see Inventory module.
/// </summary>
public sealed class Product
{
    public const int SkuMaxLength = 64;
    public const int NameMaxLength = 200;

    public Guid Id { get; private set; }

    /// <summary>Business key; unique in persistence.</summary>
    public string Sku { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    /// <summary>Unit price in the system's default currency.</summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// When available quantity falls at or below this value, low-stock rules may apply.
    /// </summary>
    public int ReorderThreshold { get; private set; }

    /// <summary>EF Core / serialization.</summary>
    private Product()
    {
    }

    public Product(Guid id, string sku, string name, decimal price, int reorderThreshold)
    {
        Id = id;
        ApplySku(sku);
        ApplyName(name);
        ValidatePrice(price);
        Price = decimal.Round(price, 2, MidpointRounding.AwayFromZero);
        ApplyReorderThreshold(reorderThreshold);
    }

    public static Product Create(string sku, string name, decimal price, int reorderThreshold)
        => new(Guid.NewGuid(), sku, name, price, reorderThreshold);

    public void Rename(string name) => ApplyName(name);

    public void ChangeSku(string sku) => ApplySku(sku);

    public void UpdatePrice(decimal price)
    {
        ValidatePrice(price);
        Price = decimal.Round(price, 2, MidpointRounding.AwayFromZero);
    }

    public void UpdateReorderThreshold(int reorderThreshold) => ApplyReorderThreshold(reorderThreshold);

    private void ApplySku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new DomainException("SKU is required.");
        }

        var trimmed = sku.Trim();
        if (trimmed.Length > SkuMaxLength)
        {
            throw new DomainException($"SKU cannot exceed {SkuMaxLength} characters.");
        }

        Sku = trimmed;
    }

    private void ApplyName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product name is required.");
        }

        var trimmed = name.Trim();
        if (trimmed.Length > NameMaxLength)
        {
            throw new DomainException($"Name cannot exceed {NameMaxLength} characters.");
        }

        Name = trimmed;
    }

    private static void ValidatePrice(decimal price)
    {
        if (price < 0)
        {
            throw new DomainException("Price cannot be negative.");
        }

        if (decimal.Round(price, 4) != price)
        {
            throw new DomainException("Price cannot have more than 4 decimal places.");
        }
    }

    private void ApplyReorderThreshold(int reorderThreshold)
    {
        if (reorderThreshold < 0)
        {
            throw new DomainException("Reorder threshold cannot be negative.");
        }

        ReorderThreshold = reorderThreshold;
    }
}

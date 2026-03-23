using OrderInventoryPlatform.Domain.Common;

namespace OrderInventoryPlatform.Domain.Inventory;

/// <summary>
/// Holds available quantity for a single catalog product. One row per product in typical designs.
/// </summary>
public sealed class InventoryItem
{
    public Guid Id { get; private set; }

    /// <summary>References <see cref="Catalog.Product.Id"/>.</summary>
    public Guid ProductId { get; private set; }

    public int AvailableQuantity { get; private set; }

    private InventoryItem()
    {
    }

    public static InventoryItem CreateForProduct(Guid productId, int initialQuantity = 0)
    {
        if (productId == Guid.Empty)
        {
            throw new DomainException("Product id is required.");
        }

        ValidateNonNegative(initialQuantity);

        return new InventoryItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            AvailableQuantity = initialQuantity
        };
    }

    public bool CanDecrease(int quantity) =>
        quantity > 0 && AvailableQuantity >= quantity;

    /// <summary>Removes stock (e.g. order fulfillment). Throws if insufficient.</summary>
    public void DecreaseAvailable(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Decrease quantity must be greater than zero.");
        }

        if (!CanDecrease(quantity))
        {
            throw new DomainException(
                $"Insufficient stock. Requested: {quantity}, available: {AvailableQuantity}.");
        }

        AvailableQuantity -= quantity;
    }

    /// <summary>Adds stock (e.g. receiving goods).</summary>
    public void IncreaseAvailable(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Increase quantity must be greater than zero.");
        }

        AvailableQuantity += quantity;
    }

    /// <summary>Applies a signed adjustment (positive adds, negative subtracts).</summary>
    public void ApplyAdjustment(int signedDelta)
    {
        if (signedDelta == 0)
        {
            throw new DomainException("Adjustment cannot be zero.");
        }

        var next = AvailableQuantity + signedDelta;
        if (next < 0)
        {
            throw new DomainException(
                $"Adjustment would make inventory negative (current: {AvailableQuantity}, delta: {signedDelta}).");
        }

        AvailableQuantity = next;
    }

    /// <summary>True when at or below the catalog reorder threshold (caller passes threshold).</summary>
    public bool IsLowStock(int reorderThreshold) =>
        reorderThreshold >= 0 && AvailableQuantity <= reorderThreshold;

    private static void ValidateNonNegative(int value)
    {
        if (value < 0)
        {
            throw new DomainException("Available quantity cannot be negative.");
        }
    }
}

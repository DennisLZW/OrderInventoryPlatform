using OrderInventoryPlatform.Domain.Common;

namespace OrderInventoryPlatform.Domain.Inventory;

/// <summary>
/// Immutable audit record of a stock change. Created via factories to enforce invariants per movement type.
/// </summary>
public sealed class InventoryMovement
{
    public Guid Id { get; private set; }

    public Guid ProductId { get; private set; }

    /// <summary>
    /// For <see cref="InventoryMovementType.In"/> and <see cref="InventoryMovementType.Out"/>, this is a positive magnitude.
    /// For <see cref="InventoryMovementType.Adjustment"/>, this may be positive or negative.
    /// </summary>
    public int Quantity { get; private set; }

    public InventoryMovementType Type { get; private set; }

    /// <summary>Optional link to the order that caused an outbound movement.</summary>
    public Guid? RelatedOrderId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>Human-readable explanation for <see cref="InventoryMovementType.Adjustment"/>.</summary>
    public string Reason { get; private set; } = string.Empty;

    public const int ReasonMaxLength = 500;

    private InventoryMovement()
    {
    }

    public static InventoryMovement CreateInbound(Guid productId, int quantity, DateTime? createdAtUtc = null)
    {
        ValidateProduct(productId);
        ValidatePositiveMagnitude(quantity, nameof(quantity));

        return new InventoryMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = quantity,
            Type = InventoryMovementType.In,
            RelatedOrderId = null,
            CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow
        };
    }

    public static InventoryMovement CreateOutbound(Guid productId, int quantity, Guid relatedOrderId, DateTime? createdAtUtc = null)
    {
        ValidateProduct(productId);
        ValidatePositiveMagnitude(quantity, nameof(quantity));

        if (relatedOrderId == Guid.Empty)
        {
            throw new DomainException("Related order id is required for outbound movements.");
        }

        return new InventoryMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = quantity,
            Type = InventoryMovementType.Out,
            RelatedOrderId = relatedOrderId,
            CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow
        };
    }

    public static InventoryMovement CreateAdjustment(
        Guid productId,
        int signedQuantity,
        string reason,
        DateTime? createdAtUtc = null)
    {
        ValidateProduct(productId);

        if (signedQuantity == 0)
        {
            throw new DomainException("Adjustment quantity cannot be zero.");
        }

        ValidateReason(reason);

        return new InventoryMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = signedQuantity,
            Type = InventoryMovementType.Adjustment,
            RelatedOrderId = null,
            CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow,
            Reason = reason.Trim()
        };
    }

    private static void ValidateReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("Adjustment reason is required.");
        }

        if (reason.Trim().Length > ReasonMaxLength)
        {
            throw new DomainException($"Adjustment reason cannot exceed {ReasonMaxLength} characters.");
        }
    }

    private static void ValidateProduct(Guid productId)
    {
        if (productId == Guid.Empty)
        {
            throw new DomainException("Product id is required.");
        }
    }

    private static void ValidatePositiveMagnitude(int value, string paramName)
    {
        if (value <= 0)
        {
            throw new DomainException($"{paramName} must be greater than zero for this movement type.");
        }
    }
}

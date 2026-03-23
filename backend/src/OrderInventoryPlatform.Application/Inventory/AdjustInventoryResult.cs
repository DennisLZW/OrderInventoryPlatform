namespace OrderInventoryPlatform.Application.Inventory;

public sealed record AdjustInventoryResponse(
    Guid ProductId,
    int AvailableQuantityAfter,
    Guid MovementId,
    string Reason);

/// <summary>
/// Result of an inventory adjustment — success with payload or a structured business failure.
/// </summary>
public abstract record AdjustInventoryResult
{
    public sealed record Success(AdjustInventoryResponse Value) : AdjustInventoryResult;

    public sealed record Failure(string Code, string Message, IReadOnlyList<string>? Details = null) : AdjustInventoryResult;
}

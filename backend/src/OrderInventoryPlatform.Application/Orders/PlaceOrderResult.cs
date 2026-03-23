namespace OrderInventoryPlatform.Application.Orders;

public sealed record PlaceOrderResponse(Guid OrderId, decimal TotalAmount);

/// <summary>
/// Result of attempting to place an order — success with payload or a structured business failure.
/// </summary>
public abstract record PlaceOrderResult
{
    public sealed record Success(PlaceOrderResponse Value) : PlaceOrderResult;

    public sealed record Failure(string Code, string Message, IReadOnlyList<string>? Details = null) : PlaceOrderResult;
}

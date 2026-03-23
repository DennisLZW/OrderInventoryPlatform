namespace OrderInventoryPlatform.Application.Orders;

/// <summary>
/// Command to place an order with one or more lines (same product may appear multiple times — quantities are aggregated before processing).
/// </summary>
public sealed record PlaceOrderCommand(IReadOnlyList<PlaceOrderLineItem> Lines);

public sealed record PlaceOrderLineItem(Guid ProductId, int Quantity);

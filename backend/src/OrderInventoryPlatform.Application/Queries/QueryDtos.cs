namespace OrderInventoryPlatform.Application.Queries;

public sealed record ProductQueryDto(
    Guid Id,
    string Sku,
    string Name,
    decimal Price,
    int ReorderThreshold);

public sealed record InventoryQueryDto(
    Guid ProductId,
    string ProductName,
    string Sku,
    int AvailableQuantity,
    int ReorderThreshold);

public sealed record OrderListItemQueryDto(
    Guid Id,
    DateTime CreatedAt,
    decimal TotalAmount,
    string Status);

public sealed record OrderLineQueryDto(
    Guid ProductId,
    string ProductName,
    string Sku,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public sealed record OrderDetailsQueryDto(
    Guid Id,
    DateTime CreatedAt,
    decimal TotalAmount,
    string Status,
    IReadOnlyList<OrderLineQueryDto> Lines);

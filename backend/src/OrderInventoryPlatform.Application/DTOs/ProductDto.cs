namespace OrderInventoryPlatform.Application.DTOs;

public record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    decimal Price,
    int ReorderThreshold);

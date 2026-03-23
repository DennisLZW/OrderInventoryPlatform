namespace OrderInventoryPlatform.Application.Queries;

public interface IProductQueryService
{
    Task<IReadOnlyList<ProductQueryDto>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface IInventoryQueryService
{
    Task<IReadOnlyList<InventoryQueryDto>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface IOrderQueryService
{
    Task<IReadOnlyList<OrderListItemQueryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OrderDetailsQueryDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
}

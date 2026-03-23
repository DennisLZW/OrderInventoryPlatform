using OrderInventoryPlatform.Domain.Catalog;

namespace OrderInventoryPlatform.Application.Interfaces;

public interface IProductRepository
{
    Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads products by id (read-only). Missing ids are omitted from the result.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, Product>> GetByIdsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default);
}

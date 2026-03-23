using OrderInventoryPlatform.Domain.Inventory;

namespace OrderInventoryPlatform.Application.Interfaces;

public interface IInventoryItemRepository
{
    /// <summary>
    /// Loads inventory rows for update (tracked). Missing product ids are omitted.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, InventoryItem>> GetTrackedByProductIdsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default);
}

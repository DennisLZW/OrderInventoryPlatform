using Microsoft.EntityFrameworkCore;
using OrderInventoryPlatform.Application.Interfaces;
using OrderInventoryPlatform.Domain.Inventory;
using OrderInventoryPlatform.Infrastructure.Persistence;

namespace OrderInventoryPlatform.Infrastructure.Persistence.Repositories;

public class InventoryItemRepository(ApplicationDbContext dbContext) : IInventoryItemRepository
{
    public async Task<IReadOnlyDictionary<Guid, InventoryItem>> GetTrackedByProductIdsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default)
    {
        if (productIds.Count == 0)
        {
            return new Dictionary<Guid, InventoryItem>();
        }

        var list = await dbContext.InventoryItems
            .Where(i => productIds.Contains(i.ProductId))
            .ToListAsync(cancellationToken);

        return list.ToDictionary(i => i.ProductId);
    }
}

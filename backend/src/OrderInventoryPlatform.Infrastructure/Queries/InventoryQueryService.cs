using Microsoft.EntityFrameworkCore;
using OrderInventoryPlatform.Application.Queries;
using OrderInventoryPlatform.Infrastructure.Persistence;

namespace OrderInventoryPlatform.Infrastructure.Queries;

public sealed class InventoryQueryService(ApplicationDbContext dbContext) : IInventoryQueryService
{
    public async Task<IReadOnlyList<InventoryQueryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await (
            from inventory in dbContext.InventoryItems.AsNoTracking()
            join product in dbContext.Products.AsNoTracking()
                on inventory.ProductId equals product.Id
            orderby product.Name
            select new InventoryQueryDto(
                inventory.ProductId,
                product.Name,
                product.Sku,
                inventory.AvailableQuantity,
                product.ReorderThreshold))
            .ToListAsync(cancellationToken);
    }
}

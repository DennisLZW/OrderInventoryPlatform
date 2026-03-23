using Microsoft.EntityFrameworkCore;
using OrderInventoryPlatform.Application.Queries;
using OrderInventoryPlatform.Infrastructure.Persistence;

namespace OrderInventoryPlatform.Infrastructure.Queries;

public sealed class ProductQueryService(ApplicationDbContext dbContext) : IProductQueryService
{
    public async Task<IReadOnlyList<ProductQueryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new ProductQueryDto(
                p.Id,
                p.Sku,
                p.Name,
                p.Price,
                p.ReorderThreshold))
            .ToListAsync(cancellationToken);
    }
}

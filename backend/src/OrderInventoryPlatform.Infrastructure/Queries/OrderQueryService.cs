using Microsoft.EntityFrameworkCore;
using OrderInventoryPlatform.Application.Queries;
using OrderInventoryPlatform.Infrastructure.Persistence;

namespace OrderInventoryPlatform.Infrastructure.Queries;

public sealed class OrderQueryService(ApplicationDbContext dbContext) : IOrderQueryService
{
    public async Task<IReadOnlyList<OrderListItemQueryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var totalsQuery =
            from line in dbContext.OrderLines.AsNoTracking()
            group line by line.OrderId
            into grouped
            select new
            {
                OrderId = grouped.Key,
                Total = grouped.Sum(x => x.LineTotal)
            };

        var rows = await (
            from order in dbContext.Orders.AsNoTracking()
            join totals in totalsQuery on order.Id equals totals.OrderId into totalsGroup
            from total in totalsGroup.DefaultIfEmpty()
            orderby order.CreatedAtUtc descending
            select new
            {
                order.Id,
                order.CreatedAtUtc,
                order.Status,
                TotalAmount = total == null ? 0m : total.Total
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new OrderListItemQueryDto(
                x.Id,
                x.CreatedAtUtc,
                x.TotalAmount,
                x.Status.ToString()))
            .ToList();
    }

    public async Task<OrderDetailsQueryDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var orderRow = await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Id == orderId)
            .Select(o => new
            {
                o.Id,
                o.CreatedAtUtc,
                o.Status
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (orderRow is null)
        {
            return null;
        }

        var lines = await (
            from line in dbContext.OrderLines.AsNoTracking()
            join product in dbContext.Products.AsNoTracking()
                on line.ProductId equals product.Id
            where line.OrderId == orderId
            orderby product.Name
            select new OrderLineQueryDto(
                line.ProductId,
                product.Name,
                product.Sku,
                line.Quantity,
                line.UnitPrice,
                line.LineTotal))
            .ToListAsync(cancellationToken);

        var totalAmount = lines.Sum(l => l.LineTotal);

        return new OrderDetailsQueryDto(
            orderRow.Id,
            orderRow.CreatedAtUtc,
            totalAmount,
            orderRow.Status.ToString(),
            lines);
    }
}

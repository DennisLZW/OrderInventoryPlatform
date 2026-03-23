using OrderInventoryPlatform.Application.Interfaces;
using OrderInventoryPlatform.Domain.Inventory;
using OrderInventoryPlatform.Domain.Orders;
using OrderInventoryPlatform.Infrastructure.Persistence;

namespace OrderInventoryPlatform.Infrastructure.Persistence.Repositories;

public sealed class OrderWriteRepository(ApplicationDbContext dbContext) : IOrderWriteRepository
{
    public void AddOrder(Order order) => dbContext.Orders.Add(order);

    public void AddMovements(IEnumerable<InventoryMovement> movements) =>
        dbContext.InventoryMovements.AddRange(movements);
}

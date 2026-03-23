using OrderInventoryPlatform.Application.Interfaces;
using OrderInventoryPlatform.Domain.Inventory;
using OrderInventoryPlatform.Infrastructure.Persistence;

namespace OrderInventoryPlatform.Infrastructure.Persistence.Repositories;

public sealed class InventoryWriteRepository(ApplicationDbContext dbContext) : IInventoryWriteRepository
{
    public void AddInventoryItem(InventoryItem item) => dbContext.InventoryItems.Add(item);

    public void AddMovement(InventoryMovement movement) => dbContext.InventoryMovements.Add(movement);
}

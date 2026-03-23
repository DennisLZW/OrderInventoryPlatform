using OrderInventoryPlatform.Domain.Inventory;
using OrderInventoryPlatform.Domain.Orders;

namespace OrderInventoryPlatform.Application.Interfaces;

/// <summary>
/// Persists new orders (with lines) and inventory movements in the same DbContext.
/// </summary>
public interface IOrderWriteRepository
{
    void AddOrder(Order order);

    void AddMovements(IEnumerable<InventoryMovement> movements);
}

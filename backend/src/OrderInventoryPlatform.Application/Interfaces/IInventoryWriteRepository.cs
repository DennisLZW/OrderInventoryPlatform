using OrderInventoryPlatform.Domain.Inventory;

namespace OrderInventoryPlatform.Application.Interfaces;

/// <summary>
/// Persists new inventory rows and movements in the same DbContext as orders.
/// </summary>
public interface IInventoryWriteRepository
{
    void AddInventoryItem(InventoryItem item);

    void AddMovement(InventoryMovement movement);
}

using OrderInventoryPlatform.Application.Inventory;

namespace OrderInventoryPlatform.Application.Interfaces;

public interface IAdjustInventoryHandler
{
    Task<AdjustInventoryResult> HandleAsync(AdjustInventoryCommand command, CancellationToken cancellationToken = default);
}

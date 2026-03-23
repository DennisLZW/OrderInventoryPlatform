namespace OrderInventoryPlatform.Application.Inventory;

public sealed record AdjustInventoryCommand(Guid ProductId, int QuantityDelta, string Reason);

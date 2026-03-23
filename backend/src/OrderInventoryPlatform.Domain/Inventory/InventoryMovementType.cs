namespace OrderInventoryPlatform.Domain.Inventory;

public enum InventoryMovementType
{
    /// <summary>Stock received (purchase, return-to-stock, etc.).</summary>
    In = 1,

    /// <summary>Stock removed for an order or shipment.</summary>
    Out = 2,

    /// <summary>Manual correction; quantity may increase or decrease.</summary>
    Adjustment = 3
}

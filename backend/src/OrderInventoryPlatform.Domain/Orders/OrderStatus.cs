namespace OrderInventoryPlatform.Domain.Orders;

public enum OrderStatus
{
    /// <summary>Order is being built; lines may still be added.</summary>
    Pending = 0,

    /// <summary>Order has been accepted and inventory should be committed.</summary>
    Placed = 1,

    /// <summary>Order was cancelled before placement.</summary>
    Cancelled = 2
}

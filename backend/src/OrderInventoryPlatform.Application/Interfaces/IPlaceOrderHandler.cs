using OrderInventoryPlatform.Application.Orders;

namespace OrderInventoryPlatform.Application.Interfaces;

public interface IPlaceOrderHandler
{
    Task<PlaceOrderResult> HandleAsync(PlaceOrderCommand command, CancellationToken cancellationToken = default);
}

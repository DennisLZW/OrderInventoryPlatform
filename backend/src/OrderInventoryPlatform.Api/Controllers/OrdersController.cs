using Microsoft.AspNetCore.Mvc;
using OrderInventoryPlatform.Application.Interfaces;
using OrderInventoryPlatform.Application.Orders;
using OrderInventoryPlatform.Application.Queries;

namespace OrderInventoryPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController(
    IPlaceOrderHandler placeOrderHandler,
    IOrderQueryService orderQueryService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderListItemQueryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderListItemQueryDto>>> GetOrders(CancellationToken cancellationToken)
    {
        var orders = await orderQueryService.GetAllAsync(cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDetailsQueryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDetailsQueryDto>> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        var order = await orderQueryService.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    /// <summary>Places an order: validates stock, persists order lines, deducts inventory, records OUT movements.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PlaceOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PlaceOrderErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> PlaceOrder(
        [FromBody] PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Lines is null || request.Lines.Count == 0)
        {
            return BadRequest(new PlaceOrderErrorResponse(
                "ORDER_LINES_EMPTY",
                "At least one order line is required.",
                null));
        }

        var command = new PlaceOrderCommand(
            request.Lines
                .Select(l => new PlaceOrderLineItem(l.ProductId, l.Quantity))
                .ToList());

        var result = await placeOrderHandler.HandleAsync(command, cancellationToken);

        return result switch
        {
            PlaceOrderResult.Success s => Ok(new PlaceOrderResponse(s.Value.OrderId, s.Value.TotalAmount)),
            PlaceOrderResult.Failure f => BadRequest(new PlaceOrderErrorResponse(f.Code, f.Message, f.Details)),
            _ => Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}

public sealed class PlaceOrderRequest
{
    public required IReadOnlyList<PlaceOrderLineRequest> Lines { get; init; }
}

public sealed class PlaceOrderLineRequest
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}

public sealed record PlaceOrderResponse(Guid OrderId, decimal TotalAmount);

public sealed record PlaceOrderErrorResponse(string Code, string Message, IReadOnlyList<string>? Details);

using Microsoft.AspNetCore.Mvc;
using OrderInventoryPlatform.Application.Interfaces;
using OrderInventoryPlatform.Application.Inventory;
using OrderInventoryPlatform.Application.Orders;
using OrderInventoryPlatform.Application.Queries;

namespace OrderInventoryPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class InventoryController(
    IInventoryQueryService inventoryQueryService,
    IAdjustInventoryHandler adjustInventoryHandler) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<InventoryQueryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InventoryQueryDto>>> GetInventory(CancellationToken cancellationToken)
    {
        var rows = await inventoryQueryService.GetAllAsync(cancellationToken);
        return Ok(rows);
    }

    /// <summary>Applies a signed stock adjustment, creating inventory if missing, and records an ADJUSTMENT movement.</summary>
    [HttpPost("adjustments")]
    [ProducesResponseType(typeof(AdjustInventoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PlaceOrderErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AdjustInventory(
        [FromBody] AdjustInventoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AdjustInventoryCommand(request.ProductId, request.QuantityDelta, request.Reason ?? string.Empty);
        var result = await adjustInventoryHandler.HandleAsync(command, cancellationToken);

        return result switch
        {
            AdjustInventoryResult.Success s => Ok(s.Value),
            AdjustInventoryResult.Failure f => BadRequest(new PlaceOrderErrorResponse(f.Code, f.Message, f.Details)),
            _ => Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}

public sealed class AdjustInventoryRequest
{
    public Guid ProductId { get; init; }
    public int QuantityDelta { get; init; }
    public required string Reason { get; init; }
}

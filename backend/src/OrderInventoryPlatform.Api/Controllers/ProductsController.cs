using Microsoft.AspNetCore.Mvc;
using OrderInventoryPlatform.Application.Queries;

namespace OrderInventoryPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductQueryService productQueryService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductQueryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductQueryDto>>> Get(CancellationToken cancellationToken)
    {
        var products = await productQueryService.GetAllAsync(cancellationToken);
        return Ok(products);
    }
}

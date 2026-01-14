using BuildingBlocks.Api.Result;
using Catalog.Api.Contracts.Products;
using Catalog.Api.Mappers.Products;
using Catalog.Application.Integrations.OneC.DTOs;


namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/catalog/products")]
//[Authorize(Policy = "CatalogRead")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    // GET /api/catalog/products?page=1&pageSize=20
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> Get(
        [FromQuery] GetProductsRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = request.ToQuery();
        var result = await sender.Send(query, cancellationToken);

        // временно,заменить на ProblemDetails
        //return BadRequest(result.Errors);

        return this.ToActionResult(result);
    }
/*
    // GET /api/catalog/products/{id}
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken ct = default)
    {
        var result = await sender.Send(new GetProductQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }
*/
}

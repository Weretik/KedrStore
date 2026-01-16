using Ardalis.Result;
using Catalog.Application.Features.Products.GetById.DTOs;
using Catalog.Application.Features.Products.GetList;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/catalog/products")]
//[Authorize(Policy = "CatalogRead")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<List<ProductListRowDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<List<ProductListRowDto>>>> Get(
        [FromQuery] GetProductsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetProductListQuery(request);
        var result = await sender.Send(query, cancellationToken);

        return this.ToActionResult(result);
    }

    // GET /api/catalog/products/{}
    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(ProductBySlugDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductBySlugDto>> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

}

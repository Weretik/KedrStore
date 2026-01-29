using Catalog.Application.Features.Products.GetById;
using Catalog.Application.Features.Products.GetById.DTOs;
using Catalog.Application.Features.Products.GetList;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/catalog/")]
//[Authorize(Policy = "CatalogRead")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    [HttpGet("products")]
    [HttpGet("{categorySlug}/products")]
    [ProducesResponseType(typeof(PagedResult<List<ProductListRowDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<List<ProductListRowDto>>>> Get(
        [FromQuery] GetProductsRequest request,
        [FromRoute] string? categorySlug,
        CancellationToken cancellationToken)
    {
        request = request with { CategorySlug = categorySlug };

        var query = new GetProductListQuery(request);
        var result = await sender.Send(query, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("product/{productSlug}")]
    [ProducesResponseType(typeof(ProductBySlugDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductBySlugDto>> GetBySlug(
        [FromRoute] string productSlug,
        [FromQuery] int priceTypeId = 10,
        CancellationToken cancellationToken = default)
    {
        var request = new GetProductBySlugRequest(productSlug, priceTypeId);
        var query = new GetProductBySlugQuery(request);
        var result = await sender.Send(query, cancellationToken);

        return this.ToActionResult(result);
    }

}

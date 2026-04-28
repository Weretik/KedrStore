using Catalog.Application.Features.Products.GetById;
using Catalog.Application.Features.Products.GetById.DTOs;
using Catalog.Application.Features.Products.GetList;
using Catalog.Application.Features.Products.GetList.DTOs;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/catalog/{lang}")]
//[Authorize(Policy = "CatalogRead")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    [HttpGet("products")]
    [HttpGet("{categorySlug}/products")]
    [ProducesResponseType(typeof(PagedResult<List<ProductListRowDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<List<ProductListRowDto>>>> Get(
        [FromQuery] GetProductsRequest request,
        [FromRoute] string lang,
        [FromRoute] string? categorySlug,
        CancellationToken cancellationToken)
    {
        request = request with { CategorySlug = categorySlug, Lang = lang };

        var query = new GetProductListQuery(request);
        var result = await sender.Send(query, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("product/{productSlug}")]
    [ProducesResponseType(typeof(ProductBySlugDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductBySlugDto>> GetBySlug(
        [FromRoute] string lang,
        [FromRoute] string productSlug,
        [FromQuery] int priceTypeId = 11,
        CancellationToken cancellationToken = default)
    {
        var request = new GetProductBySlugRequest(productSlug, priceTypeId, lang);
        var query = new GetProductBySlugQuery(request);
        var result = await sender.Send(query, cancellationToken);

        return this.ToActionResult(result);
    }
}

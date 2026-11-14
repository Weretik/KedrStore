using Catalog.Api.Features.Attributes;
using Catalog.Api.Features.Categories;
using Catalog.Api.Features.Groups;
using Catalog.Api.Features.MobileSync;
using Catalog.Api.Features.Orders;
using Catalog.Api.Features.Overview;
using Catalog.Api.Features.ProductAttributes;
using Catalog.Api.Features.SiteModels;
using Catalog.Application.Features.Products.Queries.GetProductBySlug;
using Catalog.Application.Features.Products.Queries.GetProductBySlug;
using Catalog.Application.Features.Products.Queries.GetPublicProductList;
using Catalog.Contracts.Category;
using Catalog.Contracts.Products.GetList;
using Microsoft.AspNetCore.Authorization;

namespace Catalog.Api.Features.Products;

[ApiController]
[Route("api/catalog/{lang}")]
[AllowAnonymous]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    [HttpGet("products")]
    [HttpGet("{categorySlug}/products")]
    [ProducesResponseType(typeof(PagedResult<List<ProductListRowDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<List<ProductListRowDto>>>> Get(
        [FromQuery] GetProductsRequest request,
        [FromRoute] string lang,
        [FromRoute] string? categorySlug,
        CancellationToken cancellationToken)
    {
        request = request with { CategorySlug = categorySlug, Lang = lang };

        var query = new GetPublicProductListQuery(request);
        var result = await sender.Send(query, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("product/{productSlug}")]
    [ProducesResponseType(typeof(ProductBySlugDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CatalogRedirect), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductBySlugDto>> GetBySlug(
        [FromRoute] string lang,
        [FromRoute] string productSlug,
        [FromQuery] int priceTypeId = 11,
        CancellationToken cancellationToken = default)
    {
        var request = new GetProductBySlugRequest(productSlug, priceTypeId, lang);
        var query = new GetProductBySlugQuery(request);
        var result = await sender.Send(query, cancellationToken);

        return this.ToLookupActionResult(result);
    }
}

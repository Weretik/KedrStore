using Catalog.Api.Features.Attributes;
using Catalog.Api.Features.Categories;
using Catalog.Api.Features.Groups;
using Catalog.Api.Features.MobileSync;
using Catalog.Api.Features.Orders;
using Catalog.Api.Features.Overview;
using Catalog.Api.Features.ProductAttributes;
using Catalog.Api.Features.Products;
using Catalog.Application.Features.SiteModels.Queries.GetPublicSiteModel;
using Catalog.Application.Features.SiteModels.Queries.GetPublicSiteModels;
using Catalog.Application.Features.SiteModels.Queries.GetPublicProductModelAddress;
using Catalog.Contracts.Category;
using Catalog.Contracts.SiteModels.Public;
using Microsoft.AspNetCore.Authorization;

namespace Catalog.Api.Features.SiteModels;

/// <summary>
/// The public site's models (spec catalog/007 stage 5): category lists with filters, the search, the model page, and
/// the model address of an old product address.
/// </summary>
[ApiController]
[Route("api/catalog/{lang}")]
[AllowAnonymous]
public sealed class SiteModelsController(ISender sender) : ControllerBase
{
    /// <summary>The search over all models (without attribute filters).</summary>
    [HttpGet("models", Name = "getPublicSiteModels")]
    [ProducesResponseType(typeof(PublicSiteModelListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PublicSiteModelListDto>> Search(
        [FromRoute] string lang,
        [FromQuery] GetPublicSiteModelsRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetPublicSiteModelsQuery(lang, null, request), cancellationToken));

    /// <summary>Models of a category tree; attribute filters with counts only in a final category.</summary>
    [HttpGet("{categorySlug}/models", Name = "getPublicCategorySiteModels")]
    [ProducesResponseType(typeof(PublicSiteModelListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicSiteModelListDto>> GetByCategory(
        [FromRoute] string lang,
        [FromRoute] string categorySlug,
        [FromQuery] GetPublicSiteModelsRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetPublicSiteModelsQuery(lang, categorySlug, request),
            cancellationToken));

    /// <summary>The model page; a 404 body with an address means a permanent redirect there.</summary>
    [HttpGet("model/{modelSlug}", Name = "getPublicSiteModel")]
    [ProducesResponseType(typeof(PublicSiteModelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PublicSiteModelAddressDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicSiteModelDto>> Get(
        [FromRoute] string lang,
        [FromRoute] string modelSlug,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPublicSiteModelQuery(lang, modelSlug), cancellationToken);
        if (!result.IsSuccess)
            return this.ToActionResult(result).Result ?? NotFound();

        return result.Value.Model is { } model
            ? Ok(model)
            : result.Value.Redirect is { } redirect
                ? NotFound(redirect)
                : NotFound();
    }

    /// <summary>The model address of a current or former product slug, for the permanent redirect (M-11).</summary>
    [HttpGet("product/{productSlug}/model", Name = "getPublicProductModelAddress")]
    [ProducesResponseType(typeof(PublicSiteModelAddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CatalogRedirect), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicSiteModelAddressDto>> GetProductAddress(
        [FromRoute] string lang,
        [FromRoute] string productSlug,
        CancellationToken cancellationToken)
        => this.ToLookupActionResult(await sender.Send(new GetPublicProductModelAddressQuery(productSlug),
            cancellationToken));
}

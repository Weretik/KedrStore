using Catalog.Application.Features.Attributes.Commands.CreateCatalogAttribute;
using Catalog.Application.Features.Attributes.Commands.CreateAttributeValue;
using Catalog.Application.Features.Attributes.Commands.DecideAttributeCode;
using Catalog.Application.Features.Attributes.Queries.GetCodeProducts;
using Catalog.Application.Features.Attributes.Queries.GetCatalogAttribute;
using Catalog.Application.Features.Attributes.Queries.GetCategoryAttributeFilters;
using Catalog.Application.Features.Attributes.Queries.GetCatalogAttributes;
using Catalog.Application.Features.Attributes.Queries.GetProblemReasons;
using Catalog.Application.Features.Attributes.Queries.GetUnresolvedCodes;
using Catalog.Application.Features.Attributes.Commands.SetCategoryAttributeFilters;
using Catalog.Application.Features.Attributes.Commands.UpdateCatalogAttribute;
using Catalog.Application.Features.Attributes.Commands.UpdateAttributeValue;
using Catalog.Application.Features.Attributes.Commands.UploadAttributeSwatch;
using Catalog.Contracts.Attributes;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Catalog.Api.Features.Attributes;

/// <summary>
/// The «Властивості» page (spec catalog/006, phase 3): attributes, values, codes, and category filters. Manager, Seo, and
/// Admin read it; only Admin and Seo change it (owner decisions 2026-11-05, 2026-11-06).
/// </summary>
[ApiController]
[Route("api/admin/catalog-attributes")]
[Authorize(Policy = PolicyNames.CanManageProducts)]
public sealed class AdminCatalogAttributesController(ISender sender) : ControllerBase
{
    private const long SwatchRequestLimit = UploadAttributeSwatchValidator.MaxUploadBytes + 64 * 1024;

    [HttpGet(Name = "getAdminCatalogAttributes")]
    [ProducesResponseType(typeof(List<AdminCatalogAttributeListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<AdminCatalogAttributeListItemDto>>> GetList(CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetCatalogAttributesQuery(), cancellationToken));

    [HttpGet("{id:int}", Name = "getAdminCatalogAttribute")]
    [ProducesResponseType(typeof(AdminCatalogAttributeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminCatalogAttributeDto>> Get([FromRoute] int id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetCatalogAttributeQuery(id), cancellationToken));

    /// <summary>«Нова ознака»: an attribute of the owner's own (Admin and Seo only).</summary>
    [HttpPost(Name = "createAdminCatalogAttribute")]
    [Authorize(Policy = PolicyNames.CanManageSiteCatalog)]
    [ProducesResponseType(typeof(AdminCatalogAttributeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminCatalogAttributeDto>> Create([FromBody] CreateCatalogAttributeRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new CreateCatalogAttributeCommand(request), cancellationToken));

    [HttpPut("{id:int}", Name = "updateAdminCatalogAttribute")]
    [Authorize(Policy = PolicyNames.CanManageSiteCatalog)]
    [ProducesResponseType(typeof(AdminCatalogAttributeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminCatalogAttributeDto>> Update([FromRoute] int id,
        [FromBody] UpdateCatalogAttributeRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateCatalogAttributeCommand(id, request), cancellationToken));

    [HttpPost("{id:int}/values", Name = "createAdminAttributeValue")]
    [Authorize(Policy = PolicyNames.CanManageSiteCatalog)]
    [ProducesResponseType(typeof(AdminCatalogAttributeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminCatalogAttributeDto>> CreateValue([FromRoute] int id,
        [FromBody] CreateAttributeValueRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new CreateAttributeValueCommand(id, request), cancellationToken));

    [HttpPut("{id:int}/values/{valueId:int}", Name = "updateAdminAttributeValue")]
    [Authorize(Policy = PolicyNames.CanManageSiteCatalog)]
    [ProducesResponseType(typeof(AdminCatalogAttributeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminCatalogAttributeDto>> UpdateValue([FromRoute] int id, [FromRoute] int valueId,
        [FromBody] UpdateAttributeValueRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateAttributeValueCommand(id, valueId, request),
            cancellationToken));

    /// <summary>
    /// Puts a color or decor picture into the media library (CDN folder <c>swatches</c>, library folder «Кольори»)
    /// before the value is saved; send the answer's <c>mediaFileId</c> as the value's <c>swatchMediaFileId</c>.
    /// </summary>
    [HttpPost("swatches", Name = "uploadAdminAttributeSwatch")]
    [Authorize(Policy = PolicyNames.CanManageSiteCatalog)]
    [RequestSizeLimit(SwatchRequestLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = SwatchRequestLimit)]
    [ProducesResponseType(typeof(AdminAttributeSwatchUploadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<AdminAttributeSwatchUploadDto>> UploadSwatch(IFormFile file,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(
            new UploadAttributeSwatchCommand(new SwatchUpload(file.FileName, file.Length, file.OpenReadStream)),
            cancellationToken));

    /// <summary>«Не розшифровані коди».</summary>
    [HttpGet("unresolved-codes", Name = "getAdminUnresolvedAttributeCodes")]
    [ProducesResponseType(typeof(List<AdminUnresolvedCodeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<AdminUnresolvedCodeDto>>> GetUnresolvedCodes(
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetUnresolvedCodesQuery(), cancellationToken));

    /// <summary>On-site products whose names contain the code, by category, with its own decoding there.</summary>
    [HttpGet("codes/{aliasId:int}/products", Name = "getAdminAttributeCodeProducts")]
    [ProducesResponseType(typeof(AdminCodeProductsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminCodeProductsDto>> GetCodeProducts([FromRoute] int aliasId,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetCodeProductsQuery(aliasId), cancellationToken));

    [HttpPut("codes/{aliasId:int}", Name = "decideAdminAttributeCode")]
    [Authorize(Policy = PolicyNames.CanManageSiteCatalog)]
    [ProducesResponseType(typeof(DecideAttributeCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DecideAttributeCodeResponse>> DecideCode([FromRoute] int aliasId,
        [FromBody] DecideAttributeCodeRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new DecideAttributeCodeCommand(aliasId, request), cancellationToken));

    /// <summary>Reasons of on-site products in review, for the «За причиною» filter of the product list.</summary>
    [HttpGet("problem-reasons", Name = "getAdminAttributeProblemReasons")]
    [ProducesResponseType(typeof(List<AdminProblemReasonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<AdminProblemReasonDto>>> GetProblemReasons(CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetProblemReasonsQuery(), cancellationToken));

    /// <summary>«Фільтри категорій» of one category.</summary>
    [HttpGet("category-filters/{categoryId:int}", Name = "getAdminCategoryAttributeFilters")]
    [ProducesResponseType(typeof(List<AdminCategoryAttributeFilterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<AdminCategoryAttributeFilterDto>>> GetCategoryFilters(
        [FromRoute] int categoryId, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetCategoryAttributeFiltersQuery(categoryId), cancellationToken));

    [HttpPut("category-filters/{categoryId:int}", Name = "setAdminCategoryAttributeFilters")]
    [Authorize(Policy = PolicyNames.CanManageSiteCatalog)]
    [ProducesResponseType(typeof(List<AdminCategoryAttributeFilterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<AdminCategoryAttributeFilterDto>>> SetCategoryFilters(
        [FromRoute] int categoryId, [FromBody] SetCategoryAttributeFiltersRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new SetCategoryAttributeFiltersCommand(categoryId, request),
            cancellationToken));
}

using Catalog.Application.Features.Attributes.Create;
using Catalog.Application.Features.Attributes.CreateValue;
using Catalog.Application.Features.Attributes.DecideCode;
using Catalog.Application.Features.Attributes.Get;
using Catalog.Application.Features.Attributes.GetCategoryFilters;
using Catalog.Application.Features.Attributes.GetList;
using Catalog.Application.Features.Attributes.GetProblemReasons;
using Catalog.Application.Features.Attributes.GetUnresolvedCodes;
using Catalog.Application.Features.Attributes.SetCategoryFilters;
using Catalog.Application.Features.Attributes.Update;
using Catalog.Application.Features.Attributes.UpdateValue;
using Catalog.Contracts.Attributes;
using Identity.Application.Security.Policies;
using Microsoft.AspNetCore.Authorization;

namespace Catalog.Api.Controllers;

/// <summary>
/// The «Властивості» page (spec catalog/006, phase 3): attributes, values, codes, and category filters. Manager and
/// Admin read it; only Admin changes it (owner decision 2026-11-05).
/// </summary>
[ApiController]
[Route("api/admin/catalog-attributes")]
[Authorize(Policy = PolicyNames.CanManageProducts)]
public sealed class AdminCatalogAttributesController(ISender sender) : ControllerBase
{
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

    /// <summary>«Нова ознака»: an attribute of the owner's own (Admin only).</summary>
    [HttpPost(Name = "createAdminCatalogAttribute")]
    [Authorize(Policy = PolicyNames.RequireAdminRole)]
    [ProducesResponseType(typeof(AdminCatalogAttributeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminCatalogAttributeDto>> Create([FromBody] CreateCatalogAttributeRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new CreateCatalogAttributeCommand(request), cancellationToken));

    [HttpPut("{id:int}", Name = "updateAdminCatalogAttribute")]
    [Authorize(Policy = PolicyNames.RequireAdminRole)]
    [ProducesResponseType(typeof(AdminCatalogAttributeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminCatalogAttributeDto>> Update([FromRoute] int id,
        [FromBody] UpdateCatalogAttributeRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateCatalogAttributeCommand(id, request), cancellationToken));

    [HttpPost("{id:int}/values", Name = "createAdminAttributeValue")]
    [Authorize(Policy = PolicyNames.RequireAdminRole)]
    [ProducesResponseType(typeof(AdminCatalogAttributeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminCatalogAttributeDto>> CreateValue([FromRoute] int id,
        [FromBody] CreateAttributeValueRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new CreateAttributeValueCommand(id, request), cancellationToken));

    [HttpPut("{id:int}/values/{valueId:int}", Name = "updateAdminAttributeValue")]
    [Authorize(Policy = PolicyNames.RequireAdminRole)]
    [ProducesResponseType(typeof(AdminCatalogAttributeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminCatalogAttributeDto>> UpdateValue([FromRoute] int id, [FromRoute] int valueId,
        [FromBody] UpdateAttributeValueRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateAttributeValueCommand(id, valueId, request),
            cancellationToken));

    /// <summary>«Не розшифровані коди».</summary>
    [HttpGet("unresolved-codes", Name = "getAdminUnresolvedAttributeCodes")]
    [ProducesResponseType(typeof(List<AdminUnresolvedCodeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<AdminUnresolvedCodeDto>>> GetUnresolvedCodes(
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetUnresolvedCodesQuery(), cancellationToken));

    [HttpPut("codes/{aliasId:int}", Name = "decideAdminAttributeCode")]
    [Authorize(Policy = PolicyNames.RequireAdminRole)]
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
    [Authorize(Policy = PolicyNames.RequireAdminRole)]
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

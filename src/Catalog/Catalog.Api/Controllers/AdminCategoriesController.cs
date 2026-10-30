using System.Security.Claims;
using Catalog.Application.Features.Category.AdminGetById;
using Catalog.Application.Features.Category.AdminGetList;
using Catalog.Application.Features.Category.Content.Get;
using Catalog.Application.Features.Category.Content.Save;
using Catalog.Application.Features.Slugs.Change;
using Catalog.Contracts.Category;
using Catalog.Contracts.Slugs;
using Catalog.Domain.ValueObjects;
using Identity.Application.Security.Policies;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize(Policy = PolicyNames.CanManageProducts)]
public sealed class AdminCategoriesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AdminCategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<AdminCategoryResponse>>> Get(
        [FromQuery] bool deleted,
        CancellationToken cancellationToken)
    {
        if (deleted && !User.IsInRole(RoleNames.Admin))
            return Forbid();

        var result = await sender.Send(new GetAdminCategoriesQuery(deleted), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AdminCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminCategoryResponse>> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminCategoryByIdQuery(id), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("{id:int}/slug")]
    [Authorize(Policy = PolicyNames.RequireAdminRole)]
    [ProducesResponseType(typeof(ChangeSlugResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChangeSlugResponse>> ChangeSlug(
        [FromRoute] int id,
        [FromBody] ChangeSlugRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ChangeSlugCommand(CatalogEntityType.Category, id, request), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:int}/content")]
    [ProducesResponseType(typeof(AdminCategoryContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminCategoryContentResponse>> GetContent(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCategoryContentQuery(id), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("{id:int}/content/{lang}")]
    [Authorize(Policy = PolicyNames.RequireAdminRole)]
    [ProducesResponseType(typeof(AdminCategoryContentItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminCategoryContentItem>> SaveContent(
        [FromRoute] int id,
        [FromRoute] string lang,
        [FromBody] SaveCategoryContentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? string.Empty;
        var result = await sender.Send(new SaveCategoryContentCommand(id, lang, request, userId), cancellationToken);
        return this.ToActionResult(result);
    }
}

using System.Security.Claims;
using Catalog.Application.Features.Categories.Queries.GetAdminCategoryById;
using Catalog.Application.Features.Categories.Queries.GetAdminCategories;
using Catalog.Application.Features.Categories.Queries.GetCategoryContent;
using Catalog.Application.Features.Categories.Commands.SaveCategoryContent;
using Catalog.Application.Features.Categories.Commands.SetCategorySiteVisibility;
using Catalog.Application.Features.Slugs.Commands.ChangeSlug;
using Catalog.Contracts.Category;
using Catalog.Contracts.Slugs;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;
using Identity.Contracts.Authorization;
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
        if (deleted && !User.IsInRole(RoleNames.Admin) && !User.IsInRole(RoleNames.Seo))
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
    [Authorize(Policy = PolicyNames.CanManageSiteCatalog)]
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

    [HttpPut("{id:int}/site-visibility", Name = "setAdminCategorySiteVisibility")]
    [Authorize(Policy = PolicyNames.CanManageSiteCatalog)]
    [ProducesResponseType(typeof(CategorySiteVisibilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategorySiteVisibilityResponse>> SetSiteVisibility(
        [FromRoute] int id,
        [FromBody] SetCategorySiteVisibilityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SetCategorySiteVisibilityCommand(id, request.IsShownOnSite), cancellationToken);
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
    [Authorize(Policy = PolicyNames.CanManageSiteCatalog)]
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

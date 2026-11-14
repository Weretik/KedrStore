using Catalog.Application.Features.Groups.Create;
using Catalog.Application.Features.Groups.Get;
using Catalog.Application.Features.Groups.GetList;
using Catalog.Application.Features.Groups.Rename;
using Catalog.Application.Features.Groups.SetMain;
using Catalog.Application.Features.Slugs.Change;
using Catalog.Contracts.Groups;
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

/// <summary>Variant groups (spec catalog/006): Manager and Admin edit them; only Admin and Seo change a group slug.</summary>
[ApiController]
[Route("api/admin/product-groups")]
[Authorize(Policy = PolicyNames.CanManageProducts)]
public sealed class AdminProductGroupsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getAdminProductGroups")]
    [ProducesResponseType(typeof(List<AdminProductGroupListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<AdminProductGroupListItemDto>>> GetList(
        [FromQuery] GetAdminProductGroupsRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetProductGroupsQuery(request), cancellationToken));

    [HttpGet("{id:int}", Name = "getAdminProductGroup")]
    [ProducesResponseType(typeof(AdminProductGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminProductGroupDto>> Get([FromRoute] int id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetProductGroupQuery(id), cancellationToken));

    [HttpPost(Name = "createAdminProductGroup")]
    [ProducesResponseType(typeof(AdminProductGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminProductGroupDto>> Create(
        [FromBody] CreateProductGroupRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new CreateProductGroupCommand(request), cancellationToken));

    [HttpPut("{id:int}", Name = "renameAdminProductGroup")]
    [ProducesResponseType(typeof(AdminProductGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminProductGroupDto>> Rename(
        [FromRoute] int id,
        [FromBody] RenameProductGroupRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new RenameProductGroupCommand(id, request), cancellationToken));

    [HttpPut("{id:int}/main", Name = "setAdminProductGroupMain")]
    [ProducesResponseType(typeof(AdminProductGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminProductGroupDto>> SetMain(
        [FromRoute] int id,
        [FromBody] SetProductGroupMainRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new SetProductGroupMainCommand(id, request), cancellationToken));

    [HttpPut("{id:int}/slug", Name = "changeAdminProductGroupSlug")]
    [Authorize(Policy = PolicyNames.CanManageSiteCatalog)]
    [ProducesResponseType(typeof(ChangeSlugResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChangeSlugResponse>> ChangeSlug(
        [FromRoute] int id,
        [FromBody] ChangeSlugRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(
            new ChangeSlugCommand(CatalogEntityType.ProductGroup, id, request), cancellationToken));
}

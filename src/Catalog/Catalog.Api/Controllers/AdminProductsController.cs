using Catalog.Application.Features.Groups.GetForProduct;
using Catalog.Application.Features.Groups.SetForProduct;
using Catalog.Application.Features.Products.GetAdminList;
using Catalog.Application.Features.Products.GetAdminDetail;
using Catalog.Application.Features.ProductMedia.Get;
using Catalog.Application.Features.ProductMedia.Set;
using Catalog.Application.Features.Slugs.Change;
using Catalog.Contracts.Groups;
using Catalog.Contracts.Products.GetAdminList;
using Catalog.Contracts.Products.GetAdminDetail;
using Catalog.Contracts.Products.Media;
using Catalog.Contracts.Slugs;
using Catalog.Domain.ValueObjects;
using Identity.Application.Security.Policies;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Policy = PolicyNames.CanManageProducts)]
public sealed class AdminProductsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<List<AdminProductListRowDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<List<AdminProductListRowDto>>>> Get(
        [FromQuery] GetAdminProductsRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Deleted == true && !User.IsInRole(RoleNames.Admin) && !User.IsInRole(RoleNames.Seo))
            return Forbid();

        var result = await sender.Send(new GetAdminProductListQuery(request), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("all")]
    [ProducesResponseType(typeof(List<AdminProductListRowDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AdminProductListRowDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllAdminProductsQuery(), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("by-slug/{productSlug}", Name = "getAdminProductBySlug")]
    [ProducesResponseType(typeof(AdminProductDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminProductDetailsDto>> GetBySlug(
        [FromRoute] string productSlug,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminProductBySlugQuery(productSlug), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("{id:int}/slug", Name = "changeAdminProductSlug")]
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
        var result = await sender.Send(new ChangeSlugCommand(CatalogEntityType.Product, id, request), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:int}/media", Name = "getAdminProductMedia")]
    [Authorize(Policy = PolicyNames.CanManageMedia)]
    [ProducesResponseType(typeof(ProductMediaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductMediaDto>> GetMedia(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductMediaQuery(id), cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>Replaces the photos (in order) and the scheme of a product with library files (R-31).</summary>
    [HttpPut("{id:int}/media", Name = "setAdminProductMedia")]
    [Authorize(Policy = PolicyNames.CanManageMedia)]
    [ProducesResponseType(typeof(ProductMediaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductMediaDto>> SetMedia(
        [FromRoute] int id,
        [FromBody] SetProductMediaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SetProductMediaCommand(id, request), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:int}/group", Name = "getAdminProductGroupOfProduct")]
    [ProducesResponseType(typeof(AdminProductGroupMembershipDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminProductGroupMembershipDto>> GetGroup(
        [FromRoute] int id,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetProductGroupForProductQuery(id), cancellationToken));

    /// <summary>Moves the product to a group, out of groups, or back to automatic grouping (spec catalog/006).</summary>
    [HttpPut("{id:int}/group", Name = "setAdminProductGroupOfProduct")]
    [ProducesResponseType(typeof(AdminProductGroupMembershipDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminProductGroupMembershipDto>> SetGroup(
        [FromRoute] int id,
        [FromBody] SetProductGroupRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new SetProductGroupCommand(id, request), cancellationToken));
}

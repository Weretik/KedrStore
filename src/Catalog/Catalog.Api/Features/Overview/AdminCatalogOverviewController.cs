using Catalog.Api.Features.Attributes;
using Catalog.Api.Features.Categories;
using Catalog.Api.Features.Groups;
using Catalog.Api.Features.MobileSync;
using Catalog.Api.Features.Orders;
using Catalog.Api.Features.ProductAttributes;
using Catalog.Api.Features.Products;
using Catalog.Api.Features.SiteModels;
using Catalog.Application.Features.Overview.Queries.GetAdminCatalogOverview;
using Catalog.Contracts.Overview;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Catalog.Api.Features.Overview;

/// <summary>Catalog problems shown on the admin «Огляд».</summary>
[ApiController]
[Route("api/admin/catalog/overview")]
[Authorize(Policy = PolicyNames.CanManageProducts)]
public sealed class AdminCatalogOverviewController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getAdminCatalogOverview")]
    [ProducesResponseType(typeof(AdminCatalogOverviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminCatalogOverviewResponse>> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminCatalogOverviewQuery(), cancellationToken);
        return this.ToActionResult(result);
    }
}

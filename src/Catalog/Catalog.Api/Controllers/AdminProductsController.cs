using Catalog.Application.Features.Products.GetAdminList;
using Catalog.Contracts.Products.GetAdminList;
using Microsoft.AspNetCore.Authorization;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/admin/products")]
[AllowAnonymous]
public sealed class AdminProductsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<List<AdminProductListRowDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<List<AdminProductListRowDto>>>> Get(
        [FromQuery] GetAdminProductsRequest request,
        CancellationToken cancellationToken)
    {
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
}

using Catalog.Application.Features.Products.GetAdminList;
using Catalog.Application.Features.Products.GetMissingPhotos;
using Catalog.Contracts.Products.GetAdminList;
using Catalog.Contracts.Products.GetMissingPhotos;
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

    [HttpGet("missing-photos", Name = "getAdminProductsWithUnavailablePhotos")]
    [ProducesResponseType(typeof(PagedResult<List<MissingProductPhotoListRowDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<List<MissingProductPhotoListRowDto>>>> GetMissingPhotos(
        [FromQuery] GetMissingProductPhotosHttpRequest request,
        CancellationToken cancellationToken)
    {
        var applicationRequest = new GetMissingProductPhotosRequest
        {
            Page = request.Page,
            PageSize = request.PageSize
        };
        var result = await sender.Send(new GetMissingProductPhotosQuery(applicationRequest), cancellationToken);

        return this.ToActionResult(result);
    }
}

public sealed record GetMissingProductPhotosHttpRequest
{
    [FromQuery(Name = "page")]
    public int Page { get; init; } = 1;

    [FromQuery(Name = "pageSize")]
    public int PageSize { get; init; } = 20;
}

using Catalog.Application.Features.Products.GetAdminList;
using Catalog.Application.Features.Products.GetAdminDetail;
using Catalog.Application.Features.Products.GetMissingPhotos;
using Catalog.Contracts.Products.GetAdminList;
using Catalog.Contracts.Products.GetAdminDetail;
using Catalog.Contracts.Products.GetMissingPhotos;
using Catalog.Contracts.Products.GetList;
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

    [HttpGet("missing-photos", Name = "getAdminProductsWithUnavailablePhotos")]
    [ProducesResponseType(typeof(PagedResult<List<MissingProductPhotoListRowDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<List<MissingProductPhotoListRowDto>>>> GetMissingPhotos(
        [FromQuery] GetMissingProductPhotosHttpRequest request,
        CancellationToken cancellationToken)
    {
        var applicationRequest = new GetMissingProductPhotosRequest
        {
            SearchTerm = request.SearchTerm,
            CategoryId = request.CategoryId,
            InStock = request.InStock,
            IsSale = request.IsSale,
            IsNew = request.IsNew,
            ExportToSite = request.ExportToSite,
            Sort = request.Sort,
            Page = request.Page,
            PageSize = request.PageSize
        };
        var result = await sender.Send(new GetMissingProductPhotosQuery(applicationRequest), cancellationToken);

        return this.ToActionResult(result);
    }
}

public sealed record GetMissingProductPhotosHttpRequest
{
    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }

    [FromQuery(Name = "categoryId")]
    public int? CategoryId { get; init; }

    [FromQuery(Name = "inStock")]
    public bool? InStock { get; init; }

    [FromQuery(Name = "isSale")]
    public bool? IsSale { get; init; }

    [FromQuery(Name = "isNew")]
    public bool? IsNew { get; init; }

    [FromQuery(Name = "exportToSite")]
    public bool? ExportToSite { get; init; }

    [FromQuery(Name = "sort")]
    public ProductSort Sort { get; init; } = ProductSort.IdAsc;

    [FromQuery(Name = "page")]
    public int Page { get; init; } = 1;

    [FromQuery(Name = "pageSize")]
    public int PageSize { get; init; } = 20;
}

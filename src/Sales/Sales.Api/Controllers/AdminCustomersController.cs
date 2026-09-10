using Sales.Api.Contracts.Customers;
using Sales.Application.Features.Customers.GetById;
using Sales.Application.Features.Customers.GetById.DTOs;
using Sales.Application.Features.Customers.GetList;
using Sales.Application.Features.Customers.GetList.DTOs;

namespace Sales.Api.Controllers;

[ApiController]
[Route("api/admin/customers")]
[AllowAnonymous]
public sealed class AdminCustomersController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<List<GetAdminCustomerListItemResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetList(
        [FromQuery] GetAdminCustomersRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetCustomerListQuery(new CustomerListRequest
            {
                Page = request.Page,
                PageSize = request.PageSize
            }),
            cancellationToken);

        if (result.Status != ResultStatus.Ok)
            return this.ToActionResult(result).Result!;

        var rows = result.Value.Value.Select(ToResponse).ToList();
        return Ok(new PagedResult<List<GetAdminCustomerListItemResponse>>(result.Value.PagedInfo, rows));
    }

    [HttpGet("{counterpartyId}")]
    [ProducesResponseType(typeof(GetAdminCustomerDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetById(string counterpartyId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCustomerByIdQuery(counterpartyId), cancellationToken);
        return result.Status == ResultStatus.Ok
            ? Ok(ToResponse(result.Value))
            : this.ToActionResult(result).Result!;
    }

    private static GetAdminCustomerListItemResponse ToResponse(CustomerListItemDto result)
        => new(result.CounterpartyId, result.Name, result.Phone);

    private static GetAdminCustomerDetailResponse ToResponse(CustomerDetailDto result)
        => new(
            result.CounterpartyId,
            result.Name,
            result.Phone,
            result.Email,
            result.DefaultPriceTypeId,
            result.CategoryPriceTypes
                .Select(rule => new GetAdminCustomerCategoryPriceTypeResponse(rule.CategoryId, rule.PriceTypeId))
                .ToList());
}

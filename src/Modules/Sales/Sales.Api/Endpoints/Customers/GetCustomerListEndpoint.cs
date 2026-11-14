using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Features.Customers.Queries.GetCustomerList;

namespace Sales.Api.Endpoints.Customers;

internal static class GetCustomerListEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .Produces<PagedResult<List<GetAdminCustomerListItemResponse>>>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    private static async Task<IResult> Handle(
        [AsParameters] GetAdminCustomersRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetCustomerListQuery(new CustomerListRequest
            {
                Page = request.Page ?? 1,
                PageSize = request.PageSize ?? 20,
                SearchTerm = request.SearchTerm,
                CounterpartyId = request.CounterpartyId,
                Name = request.Name,
                Phone = request.Phone,
                Sort = request.Sort ?? "name"
            }),
            cancellationToken);

        if (result.Status != ResultStatus.Ok)
            return result.ToProblem(http);

        var rows = result.Value.Value
            .Select(customer => new GetAdminCustomerListItemResponse(customer.CounterpartyId, customer.Name, customer.Phone))
            .ToList();
        return TypedResults.Ok(new PagedResult<List<GetAdminCustomerListItemResponse>>(result.Value.PagedInfo, rows));
    }
}

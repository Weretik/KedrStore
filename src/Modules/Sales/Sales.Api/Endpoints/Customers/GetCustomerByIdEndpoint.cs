using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Features.Customers.Queries.GetCustomerById;

namespace Sales.Api.Endpoints.Customers;

internal static class GetCustomerByIdEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{counterpartyId}", Handle)
            .Produces<GetAdminCustomerDetailResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] string counterpartyId, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCustomerByIdQuery(counterpartyId), cancellationToken);
        return result.Status == ResultStatus.Ok ? TypedResults.Ok(ToResponse(result.Value)) : result.ToProblem(http);
    }

    private static GetAdminCustomerDetailResponse ToResponse(CustomerDetailDto result)
        => new(
            result.CounterpartyId,
            result.Name,
            result.Phone,
            result.Email,
            result.DefaultPriceTypeId,
            result.CategoryPriceTypes
                .Select(rule => new GetAdminCustomerCategoryPriceTypeResponse(
                    rule.CategoryId, rule.PriceTypeId, rule.CategoryName, rule.PriceTypeName))
                .ToList(),
            result.DefaultPriceTypeName);
}

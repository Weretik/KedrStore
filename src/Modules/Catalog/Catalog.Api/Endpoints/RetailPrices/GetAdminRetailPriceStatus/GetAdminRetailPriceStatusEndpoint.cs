using Catalog.Application.Features.RetailPrices.Queries.GetRetailPriceStatus;
using Catalog.Contracts.RetailPrices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.RetailPrices.GetAdminRetailPriceStatus;

internal static class GetAdminRetailPriceStatusEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getAdminRetailPriceStatus")
            .WithSummary("Get the retail price type and how many of its prices the price file keeps from 1C")
            .WithDescription("Policy CanManageProducts (Manager, Seo, and Admin). The retail price type is the one the site shows (`Catalog:Pricing:RetailPriceTypeId`); its name is the price column the file must have.")
            .Produces<RetailPriceStatusDto>()
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new GetRetailPriceStatusQuery(), cancellationToken)).ToHttpResult(http);
}

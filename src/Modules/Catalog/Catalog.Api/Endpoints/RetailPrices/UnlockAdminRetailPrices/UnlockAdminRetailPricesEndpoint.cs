using Catalog.Application.Features.RetailPrices.Commands.UnlockRetailPrices;
using Catalog.Contracts.RetailPrices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.RetailPrices.UnlockAdminRetailPrices;

internal static class UnlockAdminRetailPricesEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("unlock", Handle)
            .WithName("unlockAdminRetailPrices").RequireRateLimiting("HeavyOperation")
            .WithSummary("Give the retail prices from the price file back to the 1C sync")
            .WithDescription("Policy CanManageProducts (Manager, Seo, and Admin). Removes the lock from every retail price; the amounts stay until the next 1C price sync. A repeat changes nothing. 409 when a 1C sync changed the same prices meanwhile.")
            .Produces<RetailPriceStatusDto>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new UnlockRetailPricesCommand(), cancellationToken)).ToHttpResult(http);
}

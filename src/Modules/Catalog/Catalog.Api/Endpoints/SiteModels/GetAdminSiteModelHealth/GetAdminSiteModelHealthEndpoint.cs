using Catalog.Application.Features.SiteModels.Queries.GetSiteModelHealth;
using Catalog.Contracts.SiteModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.SiteModels.GetAdminSiteModelHealth;

internal static class GetAdminSiteModelHealthEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("health", Handle)
            .WithName("getAdminSiteModelHealth")
            .Produces<SiteModelHealthDto>()
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new GetSiteModelHealthQuery(), cancellationToken)).ToHttpResult(http);
}

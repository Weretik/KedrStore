using Catalog.Application.Features.SiteModels.Queries.GetSiteShowcase;
using Catalog.Contracts.SiteModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Catalog.Api.Endpoints.SiteModels.Shared;

namespace Catalog.Api.Endpoints.SiteModels.GetAdminSiteShowcase;

internal static class GetAdminSiteShowcaseEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("showcase", Handle)
            .WithName("getAdminSiteShowcase")
            .Produces<SiteShowcaseListDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(
        [AsParameters] GetSiteModelsParameters parameters, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new GetSiteShowcaseQuery(parameters.ToShowcaseRequest()), cancellationToken))
            .ToHttpResult(http);
}

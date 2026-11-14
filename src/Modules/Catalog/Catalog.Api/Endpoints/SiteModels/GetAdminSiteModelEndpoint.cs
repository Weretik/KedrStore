using Catalog.Application.Features.SiteModels.Queries.GetSiteModel;
using Catalog.Contracts.SiteModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.SiteModels;

internal static class GetAdminSiteModelEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{id:int}", Handle)
            .WithName("getAdminSiteModel")
            .Produces<SiteModelDto>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new GetSiteModelQuery(id), cancellationToken)).ToHttpResult(http);
}

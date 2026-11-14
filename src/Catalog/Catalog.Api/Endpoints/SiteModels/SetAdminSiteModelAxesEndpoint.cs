using Catalog.Application.Features.SiteModels.Commands.SetSiteModelAxes;
using Catalog.Contracts.SiteModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.SiteModels;

internal static class SetAdminSiteModelAxesEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}/axes", Handle)
            .WithName("setAdminSiteModelAxes")
            .Produces<SiteModelDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, [FromBody] SetSiteModelAxesRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new SetSiteModelAxesCommand(id, request), cancellationToken)).ToHttpResult(http);
}

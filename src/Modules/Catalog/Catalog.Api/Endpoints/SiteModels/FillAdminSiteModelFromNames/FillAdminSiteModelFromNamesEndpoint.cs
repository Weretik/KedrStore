using Catalog.Application.Features.SiteModels.Commands.FillSiteModelFromNames;
using Catalog.Contracts.SiteModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.SiteModels.FillAdminSiteModelFromNames;

internal static class FillAdminSiteModelFromNamesEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("{id:int}/fill-from-names", Handle)
            .WithName("fillAdminSiteModelFromNames")
            .Produces<SiteModelDto>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new FillSiteModelFromNamesCommand(id), cancellationToken)).ToHttpResult(http);
}

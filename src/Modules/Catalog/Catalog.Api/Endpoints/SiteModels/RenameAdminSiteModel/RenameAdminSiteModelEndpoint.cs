using Catalog.Application.Features.SiteModels.Commands.RenameSiteModel;
using Catalog.Contracts.SiteModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.SiteModels.RenameAdminSiteModel;

internal static class RenameAdminSiteModelEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}", Handle)
            .WithName("renameAdminSiteModel")
            .Produces<SiteModelDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, [FromBody] RenameSiteModelRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new RenameSiteModelCommand(id, request), cancellationToken)).ToHttpResult(http);
}

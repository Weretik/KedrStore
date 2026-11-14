using Catalog.Application.Features.SiteModels.Commands.AcceptSiteModels;
using Catalog.Contracts.SiteModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.SiteModels.AcceptAdminSiteModels;

internal static class AcceptAdminSiteModelsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("accept", Handle)
            .WithName("acceptAdminSiteModels")
            .Produces<SiteModelHealthDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(
        [FromBody] AcceptSiteModelsRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new AcceptSiteModelsCommand(request), cancellationToken)).ToHttpResult(http);
}

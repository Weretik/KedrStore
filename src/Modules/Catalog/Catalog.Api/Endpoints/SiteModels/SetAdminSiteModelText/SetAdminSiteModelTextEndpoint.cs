using Catalog.Application.Features.SiteModels.Commands.SetSiteModelText;
using Catalog.Contracts.SiteModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.SiteModels.SetAdminSiteModelText;

internal static class SetAdminSiteModelTextEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}/texts/{language}", Handle)
            .WithName("setAdminSiteModelText")
            .Produces<SiteModelDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, [FromRoute] string language, [FromBody] SetSiteModelTextRequest request,
        ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new SetSiteModelTextCommand(id, language, request), cancellationToken))
            .ToHttpResult(http);
}

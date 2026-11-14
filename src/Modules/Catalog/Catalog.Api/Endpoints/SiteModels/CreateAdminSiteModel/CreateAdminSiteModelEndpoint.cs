using Catalog.Application.Features.SiteModels.Commands.CreateSiteModel;
using Catalog.Contracts.SiteModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.SiteModels.CreateAdminSiteModel;

internal static class CreateAdminSiteModelEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("", Handle)
            .WithName("createAdminSiteModel")
            .Produces<SiteModelDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(
        [FromBody] CreateSiteModelRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new CreateSiteModelCommand(request), cancellationToken)).ToHttpResult(http);
}

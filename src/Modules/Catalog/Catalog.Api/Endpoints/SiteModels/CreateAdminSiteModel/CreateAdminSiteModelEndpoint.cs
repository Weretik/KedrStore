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
            .WithSummary("Create a model («Нова модель»)")
            .WithDescription("The model is accepted at once. The category must be final (no children); the products (up to 200) must be in it and not in another accepted model; a proposed model gives its products up. Axes are offered from the products and saved.")
            .Produces<SiteModelDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(
        [FromBody] CreateSiteModelRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new CreateSiteModelCommand(request), cancellationToken)).ToHttpResult(http);
}

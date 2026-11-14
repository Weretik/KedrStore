using System.Security.Claims;
using Catalog.Application.Features.Categories.Commands.SetCategorySiteVisibility;
using Catalog.Contracts.Category;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Categories.SetAdminCategorySiteVisibility;

internal static class SetAdminCategorySiteVisibilityEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}/site-visibility", Handle)
            .WithName("setAdminCategorySiteVisibility")
            .RequireAuthorization(PolicyNames.CanManageSiteCatalog)
            .Produces<CategorySiteVisibilityResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, [FromBody] SetCategorySiteVisibilityRequest request, ISender sender,
        HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SetCategorySiteVisibilityCommand(id, request.IsShownOnSite), cancellationToken);
        return result.ToHttpResult(http);
    }
}

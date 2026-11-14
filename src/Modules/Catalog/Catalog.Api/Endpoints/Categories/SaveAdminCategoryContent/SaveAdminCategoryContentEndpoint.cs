using System.Security.Claims;
using Catalog.Application.Features.Categories.Commands.SaveCategoryContent;
using Catalog.Contracts.Category;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Categories.SaveAdminCategoryContent;

internal static class SaveAdminCategoryContentEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}/content/{lang}", Handle)
            .RequireAuthorization(PolicyNames.CanManageSiteCatalog)
            .Produces<AdminCategoryContentItem>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, [FromRoute] string lang, [FromBody] SaveCategoryContentRequest request,
        ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? http.User.Identity?.Name ?? string.Empty;
        var result = await sender.Send(new SaveCategoryContentCommand(id, lang, request, userId), cancellationToken);
        return result.ToHttpResult(http);
    }
}

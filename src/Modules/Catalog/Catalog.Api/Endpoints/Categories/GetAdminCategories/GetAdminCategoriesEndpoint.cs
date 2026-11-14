using System.Security.Claims;
using Catalog.Application.Features.Categories.Queries.GetAdminCategories;
using Catalog.Contracts.Category;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Categories.GetAdminCategories;

internal static class GetAdminCategoriesEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .Produces<IReadOnlyList<AdminCategoryResponse>>()
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(
        ISender sender, HttpContext http, CancellationToken cancellationToken, [FromQuery] bool? deleted = null)
    {
        // An absent flag lists live categories, as the controller's non-required parameter did.
        var listDeleted = deleted ?? false;
        if (listDeleted && !http.User.IsInRole(RoleNames.Admin) && !http.User.IsInRole(RoleNames.Seo))
            return TypedResults.Forbid();

        var result = await sender.Send(new GetAdminCategoriesQuery(listDeleted), cancellationToken);
        return result.ToHttpResult(http);
    }
}

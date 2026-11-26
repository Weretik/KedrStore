using Catalog.Application.Features.CategoryPlacement.Commands.ReorderCategory;
using Catalog.Contracts.Category;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.CategoryPlacement.ReorderAdminCategory;

internal static class ReorderAdminCategoryEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}/order", Handle)
            .WithName("reorderAdminCategory")
            .WithSummary("Move a category up or down among its neighbours")
            .WithDescription("Spec catalog/012 MC-10: direction «up» or «down». The neighbours get the order by hand, and the 1C sync keeps it. At the edge nothing changes.")
            .RequireAuthorization(PolicyNames.CanManageSiteCatalog)
            .Produces<AdminCategoryResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle([FromRoute] int id, [FromBody] ReorderCategoryRequest request,
        ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new ReorderCategoryCommand(id, request.Direction), cancellationToken)).ToHttpResult(http);
}

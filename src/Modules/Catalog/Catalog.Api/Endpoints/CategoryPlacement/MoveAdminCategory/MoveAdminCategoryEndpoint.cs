using Catalog.Application.Features.CategoryPlacement.Commands.MoveCategory;
using Catalog.Contracts.Category;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.CategoryPlacement.MoveAdminCategory;

internal static class MoveAdminCategoryEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}/parent", Handle)
            .WithName("moveAdminCategory")
            .WithSummary("Put a category under another one")
            .WithDescription("Spec catalog/011 MC-03, MC-09. Any category, from 1C or made by hand, goes under an active category without site models, or to the top without parentId; its subcategories go with it. The 1C sync keeps the place. A move under itself or its subcategory answers 400. Repeating the same request is safe.")
            .RequireAuthorization(PolicyNames.CanManageSiteCatalog)
            .Produces<AdminCategoryResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle([FromRoute] int id, [FromBody] MoveCategoryRequest request,
        ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new MoveCategoryCommand(id, request.ParentId), cancellationToken)).ToHttpResult(http);
}

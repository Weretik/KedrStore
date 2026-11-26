using Catalog.Application.Features.CategoryPlacement.Commands.DeleteManualCategory;
using Catalog.Contracts.Category;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.CategoryPlacement.DeleteAdminCategory;

internal static class DeleteAdminCategoryEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", Handle)
            .WithName("deleteAdminCategory")
            .WithSummary("Delete an empty category made by hand")
            .WithDescription("Spec catalog/011 MC-04. Only a category made by hand (400 for a 1C one) and only without subcategories, site models, or products (409). It moves to «Видалені».")
            .RequireAuthorization(PolicyNames.CanManageSiteCatalog)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

    private static async Task<IResult> Handle([FromRoute] int id, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteManualCategoryCommand(id), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToHttpResult(http);
    }
}

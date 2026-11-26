using Catalog.Application.Features.CategoryPlacement.Commands.AddCategorySiteModels;
using Catalog.Contracts.Category;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.CategoryPlacement.AddAdminCategorySiteModels;

internal static class AddAdminCategorySiteModelsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("{id:int}/site-models", Handle)
            .WithName("addAdminCategorySiteModels")
            .WithSummary("Put site models into a category")
            .WithDescription("Spec catalog/012 MC-07, spec 007 R-22. 1–200 models into a final category in one transaction; a proposed model is accepted first. A parent category answers 400, an unknown model 404. Repeating the same request is safe.")
            .RequireAuthorization(PolicyNames.CanManageSiteCatalog)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle([FromRoute] int id, [FromBody] AddCategorySiteModelsRequest request,
        ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AddCategorySiteModelsCommand(id, request.ModelIds), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToHttpResult(http);
    }
}

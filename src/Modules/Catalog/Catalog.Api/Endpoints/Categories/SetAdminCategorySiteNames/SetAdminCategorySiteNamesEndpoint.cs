using Catalog.Application.Features.Categories.Commands.SetCategorySiteNames;
using Catalog.Contracts.Category;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Categories.SetAdminCategorySiteNames;

internal static class SetAdminCategorySiteNamesEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}/site-names", Handle)
            .WithName("setAdminCategorySiteNames")
            .WithSummary("Set the Ukrainian and Russian site names of a category")
            .WithDescription("Spec catalog/010 R-02. The names win over the 1C name and the configured short name on the site, in breadcrumbs, and in titles; the 1C sync never changes them. A blank name goes back to the name from 1C at once. Repeating the same request is safe.")
            .RequireAuthorization(PolicyNames.CanManageSiteCatalog)
            .Produces<AdminCategoryResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, [FromBody] SetCategorySiteNamesRequest request, ISender sender,
        HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SetCategorySiteNamesCommand(id, request.NameUk, request.NameRu), cancellationToken);
        return result.ToHttpResult(http);
    }
}

using Catalog.Application.Features.CategoryPlacement.Commands.CreateManualCategory;
using Catalog.Contracts.Category;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.CategoryPlacement.CreateAdminCategory;

internal static class CreateAdminCategoryEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("", Handle)
            .WithName("createAdminCategory")
            .WithSummary("Create a category by hand")
            .WithDescription("Spec catalog/011 MC-01, MC-02. Under any active category without site models, or at the top without parentId. It takes the parent's «show on site» (unless isShownOnSite is sent) and «closed from search»; the 1C sync never deletes or moves it. The same Ukrainian name under the same parent answers 409.")
            .RequireAuthorization(PolicyNames.CanManageSiteCatalog)
            .Produces<AdminCategoryResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

    private static async Task<IResult> Handle([FromBody] CreateCategoryRequest request, ISender sender,
        HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new CreateManualCategoryCommand(request), cancellationToken)).ToHttpResult(http);
}

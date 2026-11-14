using Catalog.Application.Features.Attributes.Commands.SetCategoryAttributeFilters;
using Catalog.Contracts.Attributes;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Attributes.SetAdminCategoryAttributeFilters;

internal static class SetAdminCategoryAttributeFiltersEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("category-filters/{categoryId:int}", Handle)
            .WithName("setAdminCategoryAttributeFilters")
            .RequireAuthorization(PolicyNames.CanManageSiteCatalog)
            .Produces<List<AdminCategoryAttributeFilterDto>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int categoryId, [FromBody] SetCategoryAttributeFiltersRequest request, ISender sender,
        HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new SetCategoryAttributeFiltersCommand(categoryId, request),
            cancellationToken)).ToHttpResult(http);
}

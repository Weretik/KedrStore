using Catalog.Application.Features.Attributes.Queries.GetCategoryAttributeFilters;
using Catalog.Contracts.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Attributes.GetAdminCategoryAttributeFilters;

internal static class GetAdminCategoryAttributeFiltersEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("category-filters/{categoryId:int}", Handle)
            .WithName("getAdminCategoryAttributeFilters")
            .Produces<List<AdminCategoryAttributeFilterDto>>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    /// <summary>«Фільтри категорій» of one category.</summary>
    private static async Task<IResult> Handle(
        [FromRoute] int categoryId, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new GetCategoryAttributeFiltersQuery(categoryId), cancellationToken)).ToHttpResult(http);
}

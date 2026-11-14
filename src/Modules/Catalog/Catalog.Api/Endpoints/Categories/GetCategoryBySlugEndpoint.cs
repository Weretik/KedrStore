using Catalog.Application.Features.Categories.Queries.GetCategoryBySlug;
using Catalog.Contracts.Category;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.Categories;

internal static class GetCategoryBySlugEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("categories/by-slug/{categorySlug}", Handle)
            .Produces<CategoryDetailsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<CatalogRedirect>(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] string lang, [FromRoute] string categorySlug, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCategoryBySlugQuery(categorySlug, lang), cancellationToken);
        return result.ToLookupHttpResult(http);
    }
}

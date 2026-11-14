using Catalog.Application.Features.Categories.Queries.GetCategories;
using Catalog.Application.Features.Categories.Shared;
using Catalog.Contracts.Category;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.Categories;

internal static class GetCategoriesEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("categories", Handle)
            .Produces<IReadOnlyList<CategoryResponse>>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    private static async Task<IResult> Handle(
        [FromRoute] string lang, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCategoriesQuery(new CategoryFilter(), lang), cancellationToken);
        if (!result.IsSuccess) return result.ToProblem(http);
        return TypedResults.Ok(result.Value.Select(CategoryReadMapper.ToResponse).ToList());
    }
}

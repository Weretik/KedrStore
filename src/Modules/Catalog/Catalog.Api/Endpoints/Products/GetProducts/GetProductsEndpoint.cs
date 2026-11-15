using Catalog.Application.Features.Products.Queries.GetPublicProductList;
using Catalog.Contracts.Products.GetList;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Products.GetProducts;

internal static class GetProductsEndpoint
{
    /// <summary>Maps the whole catalog list and the list of one category (with its subcategories).</summary>
    public static void Map(RouteGroupBuilder group)
    {
        Describe(group.MapGet("products", HandleAll)
            .WithName("getPublicProducts")
            .WithSummary("List public products")
            .WithDescription("When `categoryId` is supplied, the page contains public products of that category and of every descendant category at any depth; sibling branches are excluded. The category subtree and all other filters are applied before `pagedInfo.totalRecords`, sorting, and paging. An unknown valid `categoryId` returns `200` with an empty page and `totalRecords = 0`."));
        Describe(group.MapGet("{categorySlug}/products", HandleCategory)
            .WithName("getPublicProductsByCategorySlug")
            .WithSummary("List public products in a category")
            .WithDescription("Returns public products of the category selected by `categorySlug` and of every descendant category at any depth; sibling branches are excluded. When `categoryId` is also supplied, it takes precedence over the slug. The category subtree and all other filters are applied before `pagedInfo.totalRecords`, sorting, and paging. A former category slug selects its category. An unknown or deleted category slug returns `404`; an unknown `categoryId` returns `200` with an empty page."));
    }

    private static void Describe(RouteHandlerBuilder endpoint) =>
        endpoint
            .Produces<PagedResult<List<ProductListRowDto>>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static Task<IResult> HandleAll(
        [AsParameters] GetProductsParameters parameters, [FromRoute] string lang, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => Handle(parameters, lang, null, sender, http, cancellationToken);

    private static Task<IResult> HandleCategory(
        [AsParameters] GetProductsParameters parameters, [FromRoute] string lang, [FromRoute] string categorySlug,
        ISender sender, HttpContext http, CancellationToken cancellationToken)
        => Handle(parameters, lang, categorySlug, sender, http, cancellationToken);

    private static async Task<IResult> Handle(
        GetProductsParameters parameters, string lang, string? categorySlug, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
    {
        var query = new GetPublicProductListQuery(parameters.ToRequest(lang, categorySlug));
        var result = await sender.Send(query, cancellationToken);

        return result.ToHttpResult(http);
    }
}

using Catalog.Contracts.Category;

namespace Catalog.Api;

internal static class CatalogLookupHttpResult
{
    /// <summary>Found → 200 with the item; gone → 404 with the redirect hint as body (or no body).</summary>
    public static IResult ToLookupHttpResult<T>(this Result<CatalogLookup<T>> result, HttpContext httpContext)
        where T : class
    {
        if (!result.IsSuccess)
            return result.ToProblem(httpContext);

        return result.Value.Item is { } item
            ? TypedResults.Ok(item)
            : result.Value.Redirect is { } redirect
                ? TypedResults.NotFound(redirect)
                : TypedResults.NotFound();
    }
}

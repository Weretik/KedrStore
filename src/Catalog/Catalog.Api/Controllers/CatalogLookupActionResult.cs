using Catalog.Contracts.Category;

namespace Catalog.Api.Controllers;

internal static class CatalogLookupActionResult
{
    /// <summary>Found → 200 with the item; gone → 404 with the redirect hint as body (or no body).</summary>
    public static ActionResult ToLookupActionResult<T>(this ControllerBase controller, Result<CatalogLookup<T>> result)
        where T : class
    {
        if (!result.IsSuccess)
            return controller.ToActionResult(result).Result ?? controller.NotFound();

        return result.Value.Item is { } item
            ? controller.Ok(item)
            : result.Value.Redirect is { } redirect
                ? controller.NotFound(redirect)
                : controller.NotFound();
    }
}

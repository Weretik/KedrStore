namespace Catalog.Contracts.Category;

/// <summary>Body of a 404 for a deleted product or category: where the client may redirect (R-016, R-018).</summary>
public sealed record CatalogRedirect(string RedirectSlug, string RedirectType = CatalogRedirect.CategoryType)
{
    public const string CategoryType = "category";
}

/// <summary>A public lookup result: the item, or a redirect hint when the item is gone.</summary>
public sealed record CatalogLookup<T>(T? Item, CatalogRedirect? Redirect) where T : class
{
    public static CatalogLookup<T> Found(T item) => new(item, null);
    public static CatalogLookup<T> Gone(CatalogRedirect? redirect) => new(null, redirect);
}

using BuildingBlocks.Application.Caching;
using Catalog.Application.Features.Categories.Shared.Tree;
using Catalog.Contracts.Slugs;

namespace Catalog.Application.Features.Slugs.Commands.ChangeSlug;

public sealed record ChangeSlugCommand(CatalogEntityType EntityType, int Id, ChangeSlugRequest Request)
    : ICommand<Result<ChangeSlugResponse>>, IInvalidatesReadCache
{
    /// <summary>A category slug is part of the public tree.</summary>
    public IReadOnlyCollection<string> InvalidatedCacheTags
        => EntityType == CatalogEntityType.Category ? [CategoryTreeCache.Tag] : [];

    public static ChangeSlugCommand ForProduct(int id, ChangeSlugRequest request) => new(CatalogEntityType.Product, id, request);
    public static ChangeSlugCommand ForCategory(int id, ChangeSlugRequest request) => new(CatalogEntityType.Category, id, request);
    public static ChangeSlugCommand ForProductGroup(int id, ChangeSlugRequest request) => new(CatalogEntityType.ProductGroup, id, request);
}

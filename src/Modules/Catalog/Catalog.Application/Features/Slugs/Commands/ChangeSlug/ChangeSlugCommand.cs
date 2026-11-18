using BuildingBlocks.Application.Caching;
using Catalog.Application.Features.Categories.Shared.Tree;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Contracts.Slugs;

namespace Catalog.Application.Features.Slugs.Commands.ChangeSlug;

public sealed record ChangeSlugCommand(CatalogEntityType EntityType, int Id, ChangeSlugRequest Request)
    : ICommand<Result<ChangeSlugResponse>>, IInvalidatesReadCache
{
    /// <summary>A category slug is part of the public tree; product and model slugs are shown by the site models pages.</summary>
    public IReadOnlyCollection<string> InvalidatedCacheTags
        => EntityType == CatalogEntityType.Category ? [CategoryTreeCache.Tag] : [SiteModelStateReader.Tag];

    public static ChangeSlugCommand ForProduct(int id, ChangeSlugRequest request) => new(CatalogEntityType.Product, id, request);
    public static ChangeSlugCommand ForCategory(int id, ChangeSlugRequest request) => new(CatalogEntityType.Category, id, request);
    public static ChangeSlugCommand ForProductGroup(int id, ChangeSlugRequest request) => new(CatalogEntityType.ProductGroup, id, request);
}

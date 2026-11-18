using BuildingBlocks.Application.Caching;

namespace Catalog.Application.Features.Categories.Shared.Tree;

/// <summary>
/// The public category tree with product counts is the same for every caller and costs a read of all categories plus a
/// count over the product read table, so it is cached per language (spec catalog/012, ADR-0016). Admin commands that
/// change it drop <see cref="Tag"/>; product counts from the 1C import (Host.Jobs, another process) show within
/// <see cref="Lifetime"/>.
/// </summary>
public static class CategoryTreeCache
{
    public const string Tag = "catalog:category-tree";
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(2);

    public static ReadCacheEntry Entry(string language, int? productTypeId)
        => ReadCacheEntry.Create("catalog.category-tree", Tag, Lifetime, $"{language}:{productTypeId}");
}

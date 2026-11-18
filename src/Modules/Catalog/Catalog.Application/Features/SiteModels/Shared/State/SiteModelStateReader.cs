using BuildingBlocks.Application.Caching;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Domain.Attributes;
using Catalog.Domain.Attributes.ValueObjects;
using Catalog.Domain.Attributes.Enums;
using Catalog.Domain.Groups;

namespace Catalog.Application.Features.SiteModels.Shared.State;

/// <summary>
/// Reads <see cref="SiteModelState"/> without tracking (database rules 1.4). One model, or the few models a command
/// changes, is read on its own: only those models, their products and those products' values. The pages that rank or
/// count the whole catalog («Моделі», «Вітрина сайту», its category tree, «Огляд») share one cached state of the whole
/// catalog for <see cref="Lifetime"/>; every command that changes what it shows drops <see cref="Tag"/>, and changes
/// from Host.Jobs (1C import, another process) show within <see cref="Lifetime"/>.
/// </summary>
public sealed class SiteModelStateReader(IReadCatalogDbContext db, IReadCache cache)
{
    public const string Tag = "catalog:site-models";
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(1);

    private static readonly ReadCacheEntry CatalogEntry = ReadCacheEntry.Create("catalog.site-models", Tag, Lifetime);

    /// <summary>The whole catalog, from the read cache.</summary>
    public async Task<SiteModelState> ReadCatalogAsync(CancellationToken cancellationToken)
        => (await cache.GetOrCreateAsync(CatalogEntry, async ct => await ReadSnapshotAsync(modelIds: null, ct),
            cancellationToken)).ToState();

    /// <summary>
    /// The whole catalog read now, for a command that answers with the counts after its own change (inside its
    /// transaction the cache still holds the state before it).
    /// </summary>
    public async Task<SiteModelState> ReadFreshCatalogAsync(CancellationToken cancellationToken)
        => (await ReadSnapshotAsync(modelIds: null, cancellationToken)).ToState();

    /// <summary>Only <paramref name="modelIds"/> with their products; other models and products are not in the state.</summary>
    public async Task<SiteModelState> ReadModelsAsync(IReadOnlyCollection<ProductGroupId> modelIds,
        CancellationToken cancellationToken)
        => (await ReadSnapshotAsync(modelIds, cancellationToken)).ToState();

    private async Task<SiteModelSnapshot> ReadSnapshotAsync(IReadOnlyCollection<ProductGroupId>? modelIds,
        CancellationToken cancellationToken)
    {
        var categories = (await db.Categories.AsNoTracking()
                .Select(category => new
                {
                    category.Id, category.ParentId, category.ShortNameUk, category.Name, category.IsShownOnSite
                })
                .ToListAsync(cancellationToken))
            .Select(category => new SiteModelSnapshot.CategoryRow(category.Id.Value, category.ParentId?.Value,
                string.IsNullOrWhiteSpace(category.ShortNameUk) ? category.Name : category.ShortNameUk,
                category.IsShownOnSite))
            .ToList();

        var models = (await ModelsQuery(modelIds).AsNoTracking().ToListAsync(cancellationToken))
            .Select(model => new SiteModelSnapshot.ModelRow(model.Id.Value, model.CategoryId.Value, model.Name,
                model.Slug, model.Status, model.AcceptedAt, model.AcceptedBy, model.MainProductId?.Value))
            .ToList();
        var members = await db.ProductGroupMembers.AsNoTracking()
            .Where(member => member.GroupId != null)
            .Where(member => modelIds == null || modelIds.Contains(member.GroupId!.Value))
            .Select(member => new { member.ProductId, GroupId = member.GroupId!.Value })
            .ToListAsync(cancellationToken);
        var memberIds = members.Select(member => member.ProductId).ToList();

        var prices = await db.ProductListProjections.AsNoTracking()
            .Where(projection => modelIds == null || memberIds.Contains(projection.ProductId))
            .Select(projection => new { projection.ProductId, projection.RetailPrice })
            .ToDictionaryAsync(projection => projection.ProductId, projection => projection.RetailPrice,
                cancellationToken);
        var products = (await db.Products.IgnoreQueryFilters().AsNoTracking()
                .Where(product => modelIds == null || memberIds.Contains(product.Id))
                .Select(product => new
                {
                    product.Id, product.Name, product.ProductSlug, product.CategoryId, product.Photo, product.Stock,
                    product.ExportToSite, product.IsDeleted
                })
                .ToListAsync(cancellationToken))
            .Select(product => new SiteModelSnapshot.ProductRow(product.Id.Value, product.Name, product.ProductSlug,
                product.CategoryId.Value, product.Photo, prices.GetValueOrDefault(product.Id), product.Stock > 0,
                product.ExportToSite, product.IsDeleted))
            .ToList();

        var axes = (await db.ProductGroupAxes.AsNoTracking()
                .Where(axis => modelIds == null || modelIds.Contains(axis.GroupId))
                .ToListAsync(cancellationToken))
            .Select(axis => new SiteModelSnapshot.AxisRow(axis.GroupId.Value, axis.AttributeId.Value, axis.Position))
            .ToList();
        var texts = (await db.ProductGroupTexts.AsNoTracking()
                .Where(text => modelIds == null || modelIds.Contains(text.GroupId))
                .ToListAsync(cancellationToken))
            .Select(text => new SiteModelSnapshot.TextRow(text.GroupId.Value, text.Language, text.ShortDescription,
                text.Description, text.SeoTitle, text.SeoDescription, text.Version, text.IsComplete))
            .ToList();
        var values = (await db.ProductAttributeValues.AsNoTracking()
                .Where(value => value.IsConfirmed && (value.ValueId != null || value.NumberValue != null))
                .Where(value => modelIds == null || memberIds.Contains(value.ProductId))
                .Select(value => new
                {
                    value.ProductId, value.AttributeId, value.Position, value.ValueId, value.NumberValue, value.Source
                })
                .ToListAsync(cancellationToken))
            .Select(value => new SiteModelSnapshot.ValueRow(value.ProductId.Value, value.AttributeId.Value,
                value.Position, value.ValueId?.Value,
                value.NumberValue?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "",
                value.Source == AttributeValueSource.Manual))
            .ToList();
        var valueIds = values.Where(value => value.ValueId != null)
            .Select(value => AttributeValueId.FromStorage(value.ValueId!.Value)).Distinct().ToList();
        var attributes = (await db.Attributes.AsNoTracking()
                .Select(attribute => new
                {
                    attribute.Id, attribute.Code, attribute.NameUk, attribute.IsVariantAxis, attribute.SortOrder,
                    attribute.Kind
                })
                .ToListAsync(cancellationToken))
            .Select(attribute => new SiteModelSnapshot.AttributeRow(attribute.Id.Value, attribute.Code,
                attribute.NameUk, attribute.IsVariantAxis, attribute.SortOrder, attribute.Kind))
            .ToList();
        var filterCounts = (await db.CategoryAttributes.AsNoTracking()
                .Where(row => row.IsFilterable)
                .GroupBy(row => row.CategoryId)
                .Select(group => new { group.Key, Count = group.Count() })
                .ToListAsync(cancellationToken))
            .Select(row => new SiteModelSnapshot.FilterCountRow(row.Key.Value, row.Count))
            .ToList();
        var valueNames = (await db.AttributeValues.AsNoTracking()
                .Where(value => modelIds == null || valueIds.Contains(value.Id))
                .Select(value => new { value.Id, value.NameUk })
                .ToListAsync(cancellationToken))
            .Select(value => new SiteModelSnapshot.ValueNameRow(value.Id.Value, value.NameUk))
            .ToList();

        return new SiteModelSnapshot(categories, products, models,
            members.Select(member => new SiteModelSnapshot.MemberRow(member.ProductId.Value, member.GroupId.Value))
                .ToList(),
            axes, texts, values, attributes, filterCounts, valueNames);
    }

    private IQueryable<ProductGroup> ModelsQuery(IReadOnlyCollection<ProductGroupId>? modelIds)
        => modelIds is null ? db.ProductGroups : db.ProductGroups.Where(model => modelIds.Contains(model.Id));
}

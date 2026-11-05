using Catalog.Application.Attributes.Import;
using Catalog.Application.Contracts.Media;
using Catalog.Application.Contracts.Persistence;
using Catalog.Contracts.Products.GetList;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.PublicListing;

/// <summary>What a list card of a variant group shows besides its product: the variant count and the colors.</summary>
public sealed record VariantGroupSummary(int VariantCount, IReadOnlyList<ProductSwatchDto> Swatches);

/// <summary>Reads variant groups for public lists: the group of each product, and the summaries of groups.</summary>
public sealed class VariantGroupReader(IReadCatalogDbContext db, IProductMediaLibrary media)
{
    /// <summary>Products that belong to a group → (group, its main product).</summary>
    public async Task<Dictionary<int, (int GroupId, int? MainProductId)>> MembershipsAsync(
        CancellationToken cancellationToken)
    {
        var rows = await (from member in db.ProductGroupMembers.AsNoTracking()
                where member.GroupId != null
                join grp in db.ProductGroups.AsNoTracking() on member.GroupId equals grp.Id
                select new { member.ProductId, grp.Id, grp.MainProductId })
            .ToListAsync(cancellationToken);
        return rows.ToDictionary(row => row.ProductId.Value, row => (row.Id.Value, row.MainProductId?.Value));
    }

    public List<CardCandidate> Candidates(IEnumerable<int> productIds,
        IReadOnlyDictionary<int, (int GroupId, int? MainProductId)> memberships)
        => productIds.Select(id => memberships.TryGetValue(id, out var group)
                ? new CardCandidate(id, group.GroupId, group.MainProductId)
                : new CardCandidate(id, null, null))
            .ToList();

    /// <summary>For each group: its variants on the site and their primary colors in dictionary order.</summary>
    public async Task<Dictionary<int, VariantGroupSummary>> SummariesAsync(IReadOnlyCollection<int> groupIds,
        string language, CancellationToken cancellationToken)
    {
        if (groupIds.Count == 0)
            return [];

        var ids = groupIds.Select(ProductGroupId.Create).Select(id => (ProductGroupId?)id).ToList();
        var variants = await (from member in db.ProductGroupMembers.AsNoTracking()
                where ids.Contains(member.GroupId)
                join projection in db.ProductListProjections.AsNoTracking() on member.ProductId equals projection.ProductId
                where projection.ExportToSite
                select new { member.ProductId, GroupId = member.GroupId!.Value })
            .ToListAsync(cancellationToken);
        var productIds = variants.Select(variant => variant.ProductId).Distinct().ToList();

        var colors = await (from value in db.ProductAttributeValues.AsNoTracking()
                where productIds.Contains(value.ProductId) && value.IsConfirmed && value.Position == 0
                      && value.ValueId != null
                join attribute in db.Attributes.AsNoTracking() on value.AttributeId equals attribute.Id
                where attribute.Code == AttributeImportCatalog.Color
                join item in db.AttributeValues.AsNoTracking() on value.ValueId equals item.Id
                select new
                {
                    value.ProductId, item.Slug, item.NameUk, item.NameRu, item.SortOrder, item.SwatchHex,
                    item.SwatchMediaFileId
                })
            .ToListAsync(cancellationToken);
        var textures = await TextureUrlsAsync(colors.Select(color => color.SwatchMediaFileId), cancellationToken);
        var colorsByProduct = colors.ToLookup(color => color.ProductId);

        return variants.GroupBy(variant => variant.GroupId.Value).ToDictionary(group => group.Key, group =>
            new VariantGroupSummary(group.Select(variant => variant.ProductId).Distinct().Count(),
                group.SelectMany(variant => colorsByProduct[variant.ProductId])
                    .DistinctBy(color => color.Slug)
                    .OrderBy(color => color.SortOrder).ThenBy(color => color.Slug, StringComparer.Ordinal)
                    .Select(color => new ProductSwatchDto(color.Slug,
                        language == "ru" && !string.IsNullOrWhiteSpace(color.NameRu) ? color.NameRu : color.NameUk,
                        color.SwatchHex,
                        color.SwatchMediaFileId is { } mediaId ? textures.GetValueOrDefault(mediaId) : null))
                    .ToList()));
    }

    /// <summary>CDN URLs of swatch textures by media file ID.</summary>
    public async Task<Dictionary<long, string>> TextureUrlsAsync(IEnumerable<long?> mediaFileIds,
        CancellationToken cancellationToken)
    {
        var ids = mediaFileIds.OfType<long>().Distinct().ToList();
        return ids.Count == 0
            ? []
            : (await media.GetByIdsAsync(ids, cancellationToken)).ToDictionary(file => file.Id, file => file.Url);
    }
}

using Catalog.Application.Abstractions.Media;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Attributes;
using Catalog.Domain.Attributes.ValueObjects;
namespace Catalog.Application.Features.Attributes.Shared.Readers;

/// <summary>Reads the «Властивості» dictionary: attributes with value and product counts, values, and aliases.</summary>
public sealed class CatalogAttributeReader(IReadCatalogDbContext db, IProductMediaLibrary media)
{
    public async Task<List<AdminCatalogAttributeListItemDto>> ListAsync(CancellationToken cancellationToken)
    {
        var attributes = await db.Attributes.AsNoTracking()
            .OrderBy(attribute => attribute.SortOrder).ThenBy(attribute => attribute.Id)
            .ToListAsync(cancellationToken);
        var valueCounts = await db.AttributeValues.AsNoTracking()
            .GroupBy(value => value.AttributeId)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Key, item => item.Count, cancellationToken);
        var productCounts = await ProductCountsAsync(null, cancellationToken);
        return attributes.Select(attribute => ToListItem(attribute,
                valueCounts.GetValueOrDefault(attribute.Id), productCounts.GetValueOrDefault(attribute.Id)))
            .ToList();
    }

    public async Task<AdminCatalogAttributeDto?> ReadAsync(AttributeId attributeId, CancellationToken cancellationToken)
    {
        var attribute = await db.Attributes.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == attributeId, cancellationToken);
        if (attribute is null)
            return null;

        var values = await db.AttributeValues.AsNoTracking()
            .Where(value => value.AttributeId == attributeId)
            .OrderBy(value => value.SortOrder).ThenBy(value => value.Id)
            .ToListAsync(cancellationToken);
        var aliases = await db.AttributeAliases.AsNoTracking()
            .Where(alias => alias.AttributeId == attributeId)
            .OrderBy(alias => alias.Scope).ThenBy(alias => alias.RawValue)
            .ToListAsync(cancellationToken);
        var valueProducts = await db.ProductAttributeValues.AsNoTracking()
            .Where(value => value.AttributeId == attributeId && value.ValueId != null)
            .GroupBy(value => value.ValueId)
            .Select(group => new { group.Key, Count = group.Select(value => value.ProductId).Distinct().Count() })
            .ToDictionaryAsync(item => item.Key!.Value, item => item.Count, cancellationToken);
        var productCount = (await ProductCountsAsync(attributeId, cancellationToken)).GetValueOrDefault(attributeId);

        var mediaIds = values.Where(value => value.SwatchMediaFileId is not null)
            .Select(value => value.SwatchMediaFileId!.Value).Distinct().ToList();
        var urls = mediaIds.Count == 0
            ? new Dictionary<long, string>()
            : (await media.GetByIdsAsync(mediaIds, cancellationToken)).ToDictionary(file => file.Id, file => file.Url);

        return new AdminCatalogAttributeDto(
            ToListItem(attribute, values.Count, productCount),
            values.Select(value => new AdminAttributeValueDto(
                value.Id.Value, value.Code, value.NameUk, value.NameRu, value.Slug, value.SortOrder, value.SwatchHex,
                value.SwatchMediaFileId,
                value.SwatchMediaFileId is { } mediaId && urls.TryGetValue(mediaId, out var url) ? url : null,
                value.IsEditedManually, valueProducts.GetValueOrDefault(value.Id))).ToList(),
            aliases.Select(alias => new AdminAttributeAliasDto(
                alias.Id.Value, alias.Scope, alias.RawValue, alias.ValueId?.Value, alias.IsConfirmed, alias.Note,
                alias.IsEditedManually)).ToList());
    }

    private async Task<Dictionary<AttributeId, int>> ProductCountsAsync(AttributeId? attributeId,
        CancellationToken cancellationToken)
    {
        var rows = db.ProductAttributeValues.AsNoTracking();
        if (attributeId is { } id)
            rows = rows.Where(value => value.AttributeId == id);

        return await rows.GroupBy(value => value.AttributeId)
            .Select(group => new { group.Key, Count = group.Select(value => value.ProductId).Distinct().Count() })
            .ToDictionaryAsync(item => item.Key, item => item.Count, cancellationToken);
    }

    private static AdminCatalogAttributeListItemDto ToListItem(CatalogAttribute attribute, int valueCount,
        int productCount)
        => new(attribute.Id.Value, attribute.Code, attribute.NameUk, attribute.NameRu,
            AttributeDtoNames.Kind(attribute.Kind), attribute.Unit, attribute.Slug, attribute.IsFilterable,
            attribute.IsVariantAxis, attribute.SortOrder, attribute.IsEditedManually, valueCount, productCount);
}

using System.Globalization;
using Catalog.Application.Attributes.Import;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Products.GetBySlug.DTOs;
using Catalog.Application.Features.Products.PublicListing;
using Catalog.Contracts.Products.GetList;
using Catalog.Domain.Entities.Attributes;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.GetBySlug;

/// <summary>
/// Reads the variant switchers and «Характеристики» of a product page (spec catalog/006, phase 5). Only values a
/// person confirmed are shown (R-06). Axes are the variant attributes whose values differ inside the group.
/// </summary>
public sealed class ProductVariantsReader(IReadCatalogDbContext db, VariantGroupReader variantGroups)
{
    private const string KeySeparator = "+";

    public async Task<(ProductVariantsDto? Variants, List<ProductCharacteristicDto> Characteristics)> ReadAsync(
        int productId, string language, CancellationToken cancellationToken)
    {
        var id = ProductId.From(productId);
        var productGroup = await (from member in db.ProductGroupMembers.AsNoTracking()
                where member.ProductId == id && member.GroupId != null
                join item in db.ProductGroups.AsNoTracking() on member.GroupId equals item.Id
                select item)
            .FirstOrDefaultAsync(cancellationToken);

        var items = productGroup is null
            ? []
            : await (from member in db.ProductGroupMembers.AsNoTracking()
                    where member.GroupId == productGroup.Id
                    join projection in db.ProductListProjections.AsNoTracking()
                        on member.ProductId equals projection.ProductId
                    where projection.ExportToSite || projection.ProductId == id
                    orderby projection.ProductId
                    select new VariantRow(projection.ProductId.Value, projection.ProductSlug,
                        language == "ru" ? projection.NameRu : projection.NameUk, projection.Photo,
                        projection.RetailPrice, projection.InStock))
                .ToListAsync(cancellationToken);

        var productIds = items.Select(item => ProductId.From(item.Id)).Append(id).Distinct().ToList();
        var values = await db.ProductAttributeValues.AsNoTracking()
            .Where(value => productIds.Contains(value.ProductId) && value.IsConfirmed)
            .ToListAsync(cancellationToken);
        var attributes = await db.Attributes.AsNoTracking().ToDictionaryAsync(attribute => attribute.Id, cancellationToken);
        var valueIds = values.Where(value => value.ValueId is not null).Select(value => value.ValueId!.Value).Distinct().ToList();
        var dictionary = await db.AttributeValues.AsNoTracking()
            .Where(value => valueIds.Contains(value.Id))
            .ToDictionaryAsync(value => value.Id, cancellationToken);
        var textures = await variantGroups.TextureUrlsAsync(dictionary.Values.Select(value => value.SwatchMediaFileId),
            cancellationToken);

        var context = new Context(language, attributes, dictionary, textures, values.ToLookup(value => value.ProductId.Value));
        var characteristics = Characteristics(context, productId);
        var variants = productGroup is null || items.Count < 2
            ? null
            : Variants(context, productGroup.Name, productGroup.Slug, productGroup.MainProductId?.Value, productId, items);
        return (variants, characteristics);
    }

    private static List<ProductCharacteristicDto> Characteristics(Context context, int productId)
        => context.Values[productId]
            .Where(value => context.Attributes.ContainsKey(value.AttributeId))
            .GroupBy(value => value.AttributeId)
            .Select(group => (Attribute: context.Attributes[group.Key], Texts: group.OrderBy(value => value.Position)
                .Select(value => context.Text(value, context.Attributes[group.Key]))
                .OfType<string>().ToList()))
            .Where(item => item.Texts.Count > 0)
            .OrderBy(item => item.Attribute.SortOrder).ThenBy(item => item.Attribute.Code, StringComparer.Ordinal)
            .Select(item => new ProductCharacteristicDto(item.Attribute.Code,
                Localized(context.Language, item.Attribute.NameUk, item.Attribute.NameRu), string.Join(" / ", item.Texts)))
            .ToList();

    private static ProductVariantsDto? Variants(Context context, string groupName, string groupSlug, int? mainId,
        int currentId, List<VariantRow> items)
    {
        var axes = new List<VariantAxisDto>();
        var keysByItem = items.ToDictionary(item => item.Id, _ => new Dictionary<string, string>(StringComparer.Ordinal));
        foreach (var attribute in context.Attributes.Values.Where(attribute => attribute.IsVariantAxis)
                     .OrderBy(attribute => attribute.SortOrder).ThenBy(attribute => attribute.Code, StringComparer.Ordinal))
        {
            var valuesByKey = new Dictionary<string, (int Order, VariantAxisValueDto Value)>(StringComparer.Ordinal);
            foreach (var item in items)
            {
                var chosen = context.Values[item.Id]
                    .Where(value => value.AttributeId == attribute.Id && value.ValueId is not null
                                    && context.Dictionary.ContainsKey(value.ValueId.Value))
                    .OrderBy(value => value.Position)
                    .Select(value => context.Dictionary[value.ValueId!.Value])
                    .ToList();
                if (chosen.Count == 0)
                    continue;

                var key = string.Join(KeySeparator, chosen.Select(value => value.Slug));
                keysByItem[item.Id][attribute.Code] = key;
                valuesByKey.TryAdd(key, (chosen[0].SortOrder, new VariantAxisValueDto(key,
                    string.Join(" / ", chosen.Select(value => Localized(context.Language, value.NameUk, value.NameRu))),
                    attribute.Code == AttributeImportCatalog.Color ? chosen.Select(context.Swatch).ToList() : [])));
            }

            if (valuesByKey.Count < 2)
            {
                foreach (var keys in keysByItem.Values)
                    keys.Remove(attribute.Code);
                continue;
            }

            axes.Add(new VariantAxisDto(attribute.Code, attribute.Slug,
                Localized(context.Language, attribute.NameUk, attribute.NameRu),
                attribute.Code == AttributeImportCatalog.Color ? "swatch" : "button",
                valuesByKey.Values.OrderBy(value => value.Order).ThenBy(value => value.Value.Key, StringComparer.Ordinal)
                    .Select(value => value.Value).ToList()));
        }

        var main = items.FirstOrDefault(item => item.Id == mainId);
        return new ProductVariantsDto(groupName, groupSlug, main?.Slug, axes,
            items.Select(item => new VariantItemDto(item.Id, item.Slug, item.Name, item.Photo, item.Price, item.InStock,
                item.Id == currentId, item.Id == mainId, keysByItem[item.Id])).ToList());
    }

    private static string Localized(string language, string uk, string? ru)
        => language == "ru" && !string.IsNullOrWhiteSpace(ru) ? ru : uk;

    private sealed record VariantRow(int Id, string Slug, string Name, string Photo, decimal? Price, bool InStock);

    private sealed record Context(
        string Language,
        Dictionary<AttributeId, CatalogAttribute> Attributes,
        Dictionary<AttributeValueId, AttributeValue> Dictionary,
        Dictionary<long, string> Textures,
        ILookup<int, ProductAttributeValue> Values)
    {
        public string? Text(ProductAttributeValue value, CatalogAttribute attribute)
        {
            if (value.ValueId is { } valueId)
                return Dictionary.TryGetValue(valueId, out var item) ? Localized(Language, item.NameUk, item.NameRu) : null;

            if (value.NumberValue is not { } number)
                return null;

            var text = number.ToString("0.###", CultureInfo.InvariantCulture);
            return string.IsNullOrWhiteSpace(attribute.Unit) ? text : $"{text} {attribute.Unit}";
        }

        public ProductSwatchDto Swatch(AttributeValue value)
            => new(value.Slug, Localized(Language, value.NameUk, value.NameRu), value.SwatchHex,
                value.SwatchMediaFileId is { } mediaId ? Textures.GetValueOrDefault(mediaId) : null);
    }
}

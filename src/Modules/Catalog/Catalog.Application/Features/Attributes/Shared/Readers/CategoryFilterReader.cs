using Catalog.Application.Abstractions.Persistence;
using Catalog.Contracts.Attributes;
namespace Catalog.Application.Features.Attributes.Shared.Readers;

/// <summary>
/// Reads «Фільтри категорій» of one category: its filter rows and, after them, attributes its products have that are
/// not offered yet (off, so a person can turn them on).
/// </summary>
public sealed class CategoryFilterReader(IReadCatalogDbContext db)
{
    public async Task<List<AdminCategoryAttributeFilterDto>?> ReadAsync(ProductCategoryId categoryId,
        CancellationToken cancellationToken)
    {
        if (!await db.Categories.AnyAsync(category => category.Id == categoryId, cancellationToken))
            return null;

        var rows = await db.CategoryAttributes.AsNoTracking()
            .Where(row => row.CategoryId == categoryId)
            .ToListAsync(cancellationToken);
        var productIds = db.Products.AsNoTracking()
            .Where(product => product.CategoryId == categoryId)
            .Select(product => product.Id);
        var counts = await db.ProductAttributeValues.AsNoTracking()
            .Where(value => productIds.Contains(value.ProductId))
            .GroupBy(value => value.AttributeId)
            .Select(group => new { group.Key, Count = group.Select(value => value.ProductId).Distinct().Count() })
            .ToDictionaryAsync(item => item.Key, item => item.Count, cancellationToken);
        var attributes = await db.Attributes.AsNoTracking().ToDictionaryAsync(item => item.Id, cancellationToken);

        var offered = rows
            .Where(row => attributes.ContainsKey(row.AttributeId))
            .OrderBy(row => row.SortOrder).ThenBy(row => row.AttributeId.Value)
            .Select(row => new AdminCategoryAttributeFilterDto(row.AttributeId.Value, attributes[row.AttributeId].Code,
                attributes[row.AttributeId].NameUk, row.IsFilterable, row.SortOrder, row.IsEditedManually,
                counts.GetValueOrDefault(row.AttributeId)));
        var offeredIds = rows.Select(row => row.AttributeId).ToHashSet();
        var candidates = counts.Keys
            .Where(id => !offeredIds.Contains(id) && attributes.ContainsKey(id))
            .Select(id => attributes[id])
            .OrderBy(attribute => attribute.SortOrder).ThenBy(attribute => attribute.Id.Value)
            .Select(attribute => new AdminCategoryAttributeFilterDto(attribute.Id.Value, attribute.Code,
                attribute.NameUk, false, attribute.SortOrder, false, counts[attribute.Id]));
        return offered.Concat(candidates).ToList();
    }
}

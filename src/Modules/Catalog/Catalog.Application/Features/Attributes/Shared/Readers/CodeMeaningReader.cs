using Catalog.Application.Abstractions.Persistence;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Attributes;
using Catalog.Domain.Attributes.ValueObjects;
namespace Catalog.Application.Features.Attributes.Shared.Readers;

/// <summary>
/// Reads what codes mean (spec catalog/009): the value of the code's own attribute (the alias for all products, the
/// category decoding for one category) followed by the other attributes the owner added. The key's category is
/// <c>null</c> for the decoding for all products.
/// </summary>
public sealed class CodeMeaningReader(IReadCatalogDbContext db)
{
    public async Task<Dictionary<(int AliasId, int? CategoryId), List<AdminCodeMeaningDto>>> ReadAsync(
        IReadOnlyCollection<AttributeAliasId> aliasIds, CancellationToken cancellationToken)
    {
        var own = await (from alias in db.AttributeAliases.AsNoTracking()
                where aliasIds.Contains(alias.Id) && alias.ValueId != null
                select new { AliasId = alias.Id, CategoryId = (ProductCategoryId?)null, alias.AttributeId, ValueId = alias.ValueId!.Value, Own = true })
            .ToListAsync(cancellationToken);
        var ownByCategory = await (from item in db.AttributeAliasCategoryValues.AsNoTracking()
                where aliasIds.Contains(item.AliasId) && item.ValueId != null
                join alias in db.AttributeAliases on item.AliasId equals alias.Id
                select new { item.AliasId, CategoryId = (ProductCategoryId?)item.CategoryId, alias.AttributeId, ValueId = item.ValueId!.Value, Own = true })
            .ToListAsync(cancellationToken);
        var extra = await db.AttributeAliasMeanings.AsNoTracking()
            .Where(item => aliasIds.Contains(item.AliasId))
            .OrderBy(item => item.Id)
            .Select(item => new { item.AliasId, item.CategoryId, item.AttributeId, item.ValueId, Own = false })
            .ToListAsync(cancellationToken);

        var rows = own.Concat(ownByCategory).Concat(extra).ToList();
        var attributeIds = rows.Select(row => row.AttributeId).Distinct().ToList();
        var valueIds = rows.Select(row => row.ValueId).Distinct().ToList();
        var attributeNames = await db.Attributes.AsNoTracking()
            .Where(attribute => attributeIds.Contains(attribute.Id))
            .ToDictionaryAsync(attribute => attribute.Id, attribute => attribute.NameUk, cancellationToken);
        var valueNames = await db.AttributeValues.AsNoTracking()
            .Where(value => valueIds.Contains(value.Id))
            .ToDictionaryAsync(value => value.Id, value => value.NameUk, cancellationToken);

        return rows
            .GroupBy(row => (row.AliasId.Value, row.CategoryId?.Value))
            .ToDictionary(group => group.Key, group => group
                .Select(row => new AdminCodeMeaningDto(row.AttributeId.Value,
                    attributeNames.GetValueOrDefault(row.AttributeId) ?? string.Empty, row.ValueId.Value,
                    valueNames.GetValueOrDefault(row.ValueId) ?? string.Empty))
                .ToList());
    }
}

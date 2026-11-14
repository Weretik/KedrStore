using Catalog.Application.Abstractions.Persistence;
using Catalog.Domain.Attributes;
using Catalog.Domain.Groups;
using Catalog.Domain.Products.Media;

namespace Catalog.Application.Features.SiteModels.Shared;

/// <summary>Reads <see cref="SiteModelState"/> without tracking.</summary>
public sealed class SiteModelStateReader(IReadCatalogDbContext db)
{
    public async Task<SiteModelState> ReadAsync(CancellationToken cancellationToken)
    {
        var categories = await db.Categories.AsNoTracking()
            .Select(category => new
            {
                category.Id, category.ParentId, category.ShortNameUk, category.Name, category.IsShownOnSite
            })
            .ToListAsync(cancellationToken);
        var shownCategories = categories.Where(category => category.IsShownOnSite)
            .Select(category => category.Id).ToHashSet();

        var prices = await db.ProductListProjections.AsNoTracking()
            .Select(projection => new { projection.ProductId, projection.RetailPrice })
            .ToDictionaryAsync(projection => projection.ProductId, projection => projection.RetailPrice,
                cancellationToken);
        var products = (await db.Products.IgnoreQueryFilters().AsNoTracking()
                .Select(product => new
                {
                    product.Id, product.Name, product.ProductSlug, product.CategoryId, product.Photo, product.Stock,
                    product.ExportToSite, product.IsDeleted
                })
                .ToListAsync(cancellationToken))
            .Select(product => new SiteModelProductFacts(product.Id, product.Name, product.ProductSlug,
                product.CategoryId, product.Photo, prices.GetValueOrDefault(product.Id), product.Stock > 0,
                product.ExportToSite, product.IsDeleted, shownCategories.Contains(product.CategoryId)))
            .ToDictionary(product => product.Id);

        var models = await db.ProductGroups.AsNoTracking().ToDictionaryAsync(model => model.Id, cancellationToken);
        var members = await db.ProductGroupMembers.AsNoTracking()
            .Where(member => member.GroupId != null)
            .Select(member => new { member.ProductId, GroupId = member.GroupId!.Value })
            .ToListAsync(cancellationToken);
        var axes = await db.ProductGroupAxes.AsNoTracking().ToListAsync(cancellationToken);
        var texts = await db.ProductGroupTexts.AsNoTracking().ToListAsync(cancellationToken);
        var values = await db.ProductAttributeValues.AsNoTracking()
            .Where(value => value.IsConfirmed && (value.ValueId != null || value.NumberValue != null))
            .Select(value => new
            {
                value.ProductId, value.AttributeId, value.Position, value.ValueId, value.NumberValue, value.Source
            })
            .ToListAsync(cancellationToken);
        var attributes = await db.Attributes.AsNoTracking()
            .Select(attribute => new SiteModelAttributeFacts(attribute.Id, attribute.Code, attribute.NameUk,
                attribute.IsVariantAxis, attribute.SortOrder, attribute.Kind))
            .ToDictionaryAsync(attribute => attribute.Id, cancellationToken);
        var filterCounts = await db.CategoryAttributes.AsNoTracking()
            .Where(row => row.IsFilterable)
            .GroupBy(row => row.CategoryId)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Key, item => item.Count, cancellationToken);
        var valueNames = await db.AttributeValues.AsNoTracking()
            .Select(value => new { value.Id, value.NameUk })
            .ToDictionaryAsync(value => value.Id, value => value.NameUk, cancellationToken);

        return new SiteModelState
        {
            Products = products,
            Models = models,
            MembersByModel = members.Where(member => models.ContainsKey(member.GroupId))
                .ToLookup(member => member.GroupId, member => member.ProductId),
            ModelOfProduct = members.Where(member => models.ContainsKey(member.GroupId))
                .ToDictionary(member => member.ProductId, member => member.GroupId),
            SavedAxes = axes.ToLookup(axis => axis.GroupId),
            Texts = texts.ToLookup(text => text.GroupId),
            Values = values.ToLookup(value => value.ProductId, value => new SiteModelValueFacts(value.AttributeId,
                value.Position, value.ValueId, value.NumberValue?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "",
                value.Source == AttributeValueSource.Manual)),
            Attributes = attributes,
            ValueNames = valueNames,
            CategoryNames = categories.ToDictionary(category => category.Id,
                category => string.IsNullOrWhiteSpace(category.ShortNameUk) ? category.Name : category.ShortNameUk),
            CategoryParents = categories.ToDictionary(category => category.Id, category => category.ParentId),
            ShownCategories = shownCategories,
            FilterCounts = filterCounts
        };
    }
}

using Catalog.Application.Attributes.Recognition;
using Catalog.Application.Contracts.Persistence;
using Catalog.Contracts.Attributes;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Attributes.GetCodeProducts;

/// <summary>
/// Lists the on-site, not deleted products whose original 1C names contain the code, grouped by their category with
/// the code's own decoding there. Categories with more products come first.
/// </summary>
public sealed class GetCodeProductsQueryHandler(IReadCatalogDbContext db)
    : IQueryHandler<GetCodeProductsQuery, Result<AdminCodeProductsDto>>
{
    public async ValueTask<Result<AdminCodeProductsDto>> Handle(GetCodeProductsQuery query,
        CancellationToken cancellationToken)
    {
        var aliasId = AttributeAliasId.Create(query.AliasId);
        var alias = await db.AttributeAliases.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == aliasId, cancellationToken);
        if (alias is null)
            return Result.NotFound();

        var products = (await db.Products.AsNoTracking()
                .Where(product => product.ExportToSite)
                .Select(product => new { product.Id, product.CategoryId, product.Name, product.Photo })
                .ToListAsync(cancellationToken))
            .Where(product => ProductAttributeRecognizer.Mentions(product.Name, alias.RawValue))
            .ToList();

        var categoryRows = await db.Categories.IgnoreQueryFilters().AsNoTracking()
            .Select(category => new { category.Id, category.ParentId, category.ShortNameUk, category.Name })
            .ToListAsync(cancellationToken);
        var paths = CategoryPaths.Build(categoryRows.Select(category => new RecognitionCategory(
            category.Id.Value, category.ParentId?.Value,
            string.IsNullOrEmpty(category.ShortNameUk) ? category.Name : category.ShortNameUk))).Paths;

        var own = await (from decision in db.AttributeAliasCategoryValues.AsNoTracking()
                where decision.AliasId == aliasId
                join value in db.AttributeValues on decision.ValueId equals value.Id into decided
                from value in decided.DefaultIfEmpty()
                select new { decision.CategoryId, decision.ValueId, ValueName = value == null ? null : value.NameUk })
            .ToListAsync(cancellationToken);
        var ownByCategory = own.ToDictionary(item => item.CategoryId.Value);

        var categories = products
            .GroupBy(product => product.CategoryId.Value)
            .Select(group =>
            {
                var decision = ownByCategory.GetValueOrDefault(group.Key);
                return new AdminCodeCategoryDto(
                    group.Key,
                    paths.GetValueOrDefault(group.Key) ?? group.Key.ToString(CultureInfo.InvariantCulture),
                    decision is not null,
                    decision?.ValueId?.Value,
                    decision?.ValueName,
                    group.OrderBy(product => product.Name, StringComparer.Ordinal)
                        .Select(product => new AdminCodeProductDto(product.Id.Value, product.Name, product.Photo))
                        .ToList());
            })
            .OrderByDescending(category => category.Products.Count)
            .ThenBy(category => category.CategoryPath, StringComparer.Ordinal)
            .ToList();

        return Result.Success(new AdminCodeProductsDto(alias.Id.Value, alias.RawValue, categories));
    }
}

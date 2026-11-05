using Catalog.Application.Querying;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Products.PublicListing;
using Catalog.Contracts.Products.GetList;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.GetPublicList;

/// <summary>
/// Public product list. With <c>filter</c> it keeps products with the chosen attribute values; with
/// <c>groupVariants</c> a variant group is one row (spec catalog/006, phase 5). Without them it is unchanged.
/// </summary>
public sealed class GetPublicProductListQueryHandler(
    IReadCatalogDbContext catalogDbContext,
    PublicProductQuery publicProducts,
    AttributeFilters attributeFilters,
    VariantGroupReader variantGroups)
    : IQueryHandler<GetPublicProductListQuery, Result<PagedResult<List<ProductListRowDto>>>>
{
    private const int MaxPageSize = 100;

    public async ValueTask<Result<PagedResult<List<ProductListRowDto>>>> Handle(
        GetPublicProductListQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var request = query.Request;
        var language = CatalogLanguage.Normalize(request.Lang);

        var projectionsQuery = await publicProducts.BuildAsync(request, language, cancellationToken);
        if (projectionsQuery is null)
            return Result.NotFound();

        var parsedFilters = AttributeFilters.Parse(request.Filter);
        if (parsedFilters is null)
            return InvalidFilter();
        var filters = await attributeFilters.ResolveAsync(parsedFilters, cancellationToken);
        if (filters is null)
            return Result.NotFound();

        projectionsQuery = attributeFilters.Apply(projectionsQuery, filters);
        projectionsQuery = ApplySorting(projectionsQuery, catalogDbContext.Categories.AsNoTracking(), request.Sort, language);

        var pageNumber = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;
        if (pageSize > MaxPageSize) pageSize = MaxPageSize;
        var skip = (pageNumber - 1) * pageSize;

        long totalRecords;
        List<ProductListRowDto> items;
        if (request.GroupVariants)
        {
            (totalRecords, items) = await GroupedPageAsync(projectionsQuery, skip, pageSize, language, cancellationToken);
        }
        else
        {
            totalRecords = await projectionsQuery.LongCountAsync(cancellationToken);
            items = await ToRows(projectionsQuery.Skip(skip).Take(pageSize), language).ToListAsync(cancellationToken);
        }

        var pagedInfo = new PagedInfo(
            pageNumber: pageNumber,
            pageSize: pageSize,
            totalPages: (long)Math.Ceiling(totalRecords / (double)pageSize),
            totalRecords: totalRecords);

        return Result.Success(new PagedResult<List<ProductListRowDto>>(pagedInfo, items));
    }

    /// <summary>One row per card: the sorted IDs are read once and collapsed by group in memory.</summary>
    private async Task<(long Total, List<ProductListRowDto> Items)> GroupedPageAsync(
        IQueryable<ProductListProjection> projectionsQuery, int skip, int pageSize, string language,
        CancellationToken cancellationToken)
    {
        var sortedIds = await projectionsQuery.Select(projection => projection.ProductId.Value).ToListAsync(cancellationToken);
        var memberships = await variantGroups.MembershipsAsync(cancellationToken);
        var cards = ProductCards.Collapse(variantGroups.Candidates(sortedIds, memberships));
        var page = cards.Skip(skip).Take(pageSize).ToList();
        if (page.Count == 0)
            return (cards.Count, []);

        var pageIds = page.Select(card => ProductId.From(card.ProductId)).ToList();
        var rows = await ToRows(catalogDbContext.ProductListProjections.AsNoTracking()
                .Where(projection => pageIds.Contains(projection.ProductId)), language)
            .ToDictionaryAsync(row => row.Id, cancellationToken);
        var summaries = await variantGroups.SummariesAsync(
            page.Where(card => card.GroupId is not null).Select(card => card.GroupId!.Value).ToList(), language,
            cancellationToken);

        var items = page.Where(card => rows.ContainsKey(card.ProductId))
            .Select(card => card.GroupId is { } groupId && summaries.TryGetValue(groupId, out var summary)
                ? rows[card.ProductId] with { VariantCount = summary.VariantCount, Swatches = summary.Swatches }
                : rows[card.ProductId])
            .ToList();
        return (cards.Count, items);
    }

    private static IQueryable<ProductListRowDto> ToRows(IQueryable<ProductListProjection> query, string language)
        => query.Select(projection => new ProductListRowDto
        {
            Id = projection.ProductId.Value,
            Name = language == "ru" ? projection.NameRu : projection.NameUk,
            ProductSlug = projection.ProductSlug,
            Photo = projection.Photo,
            CategoryId = projection.CategoryId.Value,
            InStock = projection.InStock,
            IsSale = projection.IsSale,
            IsNew = projection.IsNew,
            Price = projection.RetailPrice
        });

    private static Result InvalidFilter()
        => Result.Invalid(new ValidationError("filter",
            $"Use attribute-slug:value-slug[,value-slug], at most {AttributeFilters.MaxValues} values.",
            "Catalog.Products.InvalidFilter", ValidationSeverity.Error));

    private static IQueryable<ProductListProjection> ApplySorting(
        IQueryable<ProductListProjection> projectionsQuery,
        IQueryable<ProductCategory> categoriesQuery,
        ProductSort sort,
        string language)
    {
        return sort switch
        {
            ProductSort.IdDesc => projectionsQuery.OrderByDescending(projection => projection.ProductId),
            ProductSort.NameAsc => language == "ru"
                ? projectionsQuery.OrderBy(projection => projection.NameRu)
                : projectionsQuery.OrderBy(projection => projection.NameUk),
            ProductSort.NameDesc => language == "ru"
                ? projectionsQuery.OrderByDescending(projection => projection.NameRu)
                : projectionsQuery.OrderByDescending(projection => projection.NameUk),
            ProductSort.PriceAsc => projectionsQuery
                .OrderBy(projection => projection.RetailPrice.HasValue ? 0 : 1)
                .ThenBy(projection => projection.RetailPrice),
            ProductSort.PriceDesc => projectionsQuery
                .OrderBy(projection => projection.RetailPrice.HasValue ? 0 : 1)
                .ThenByDescending(projection => projection.RetailPrice),
            ProductSort.CategoryAsc => projectionsQuery.OrderBy(projection => categoriesQuery
                    .Where(category => category.Id == projection.CategoryId)
                    .Select(category => language == "ru" ? category.ShortNameRu : category.ShortNameUk)
                    .FirstOrDefault())
                .ThenBy(projection => projection.ProductId),
            ProductSort.CategoryDesc => projectionsQuery.OrderByDescending(projection => categoriesQuery
                    .Where(category => category.Id == projection.CategoryId)
                    .Select(category => language == "ru" ? category.ShortNameRu : category.ShortNameUk)
                    .FirstOrDefault())
                .ThenBy(projection => projection.ProductId),
            ProductSort.AvailabilityThenName => language == "ru"
                ? projectionsQuery.OrderByDescending(projection => projection.InStock)
                    .ThenBy(projection => projection.NameRu).ThenBy(projection => projection.ProductId)
                : projectionsQuery.OrderByDescending(projection => projection.InStock)
                    .ThenBy(projection => projection.NameUk).ThenBy(projection => projection.ProductId),
            _ => projectionsQuery.OrderBy(projection => projection.ProductId)
        };
    }
}

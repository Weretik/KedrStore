using Catalog.Application.Contracts.Persistence;
using Catalog.Contracts.Products.GetList;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.GetPublicList;

public sealed class GetPublicProductListQueryHandler(IReadCatalogDbContext catalogDbContext)
    : IQueryHandler<GetPublicProductListQuery, Result<PagedResult<List<ProductListRowDto>>>>
{
    private const int MaxPageSize = 100;

    public async ValueTask<Result<PagedResult<List<ProductListRowDto>>>> Handle(
        GetPublicProductListQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var request = query.Request;
        var language = NormalizeLanguage(request.Lang);

        var projectionsQuery = catalogDbContext.ProductListProjections
            .AsNoTracking()
            .Where(projection => projection.ExportToSite);
        var categoryIds = await GetSelectedCategoryTreeIdsAsync(request, cancellationToken);
        if (categoryIds is not null)
        {
            projectionsQuery = projectionsQuery.Where(projection => categoryIds.Contains(projection.CategoryId));
        }
        projectionsQuery = ApplyFilters(projectionsQuery, request, language);
        projectionsQuery = ApplySorting(projectionsQuery, catalogDbContext.Categories.AsNoTracking(), request.Sort, language);

        var pageNumber = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;
        if (pageSize > MaxPageSize) pageSize = MaxPageSize;

        var totalRecords = await projectionsQuery.LongCountAsync(cancellationToken);
        var totalPages = (long)Math.Ceiling(totalRecords / (double)pageSize);
        var skip = (pageNumber - 1) * pageSize;

        var items = await projectionsQuery
            .Skip(skip)
            .Take(pageSize)
            .Select(projection => new ProductListRowDto
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
            })
            .ToListAsync(cancellationToken);

        var pagedInfo = new PagedInfo(
            pageNumber: pageNumber,
            pageSize: pageSize,
            totalPages: totalPages,
            totalRecords: totalRecords);

        return Result.Success(new PagedResult<List<ProductListRowDto>>(pagedInfo, items));
    }

    private async Task<ProductCategoryId[]?> GetSelectedCategoryTreeIdsAsync(
        GetProductsRequest request,
        CancellationToken cancellationToken)
    {
        if (request.CategoryId is null && string.IsNullOrWhiteSpace(request.CategorySlug))
        {
            return null;
        }

        var categories = await catalogDbContext.Categories
            .AsNoTracking()
            .Select(category => new { category.Id, category.ParentId, category.Slug })
            .ToListAsync(cancellationToken);

        var selected = request.CategoryId is { } categoryId
            ? categories.FirstOrDefault(category => category.Id.Value == categoryId)
            : categories.FirstOrDefault(category => category.Slug == request.CategorySlug!.Trim());
        if (selected is null)
        {
            return [];
        }

        var childrenByParent = categories
            .Where(category => category.ParentId.HasValue)
            .ToLookup(category => category.ParentId!.Value, category => category.Id);
        var selectedIds = new HashSet<ProductCategoryId>();
        var pending = new Stack<ProductCategoryId>();
        pending.Push(selected.Id);

        while (pending.TryPop(out var currentId))
        {
            if (!selectedIds.Add(currentId)) continue;
            foreach (var childId in childrenByParent[currentId]) pending.Push(childId);
        }

        return [.. selectedIds];
    }

    private static IQueryable<ProductListProjection> ApplyFilters(
        IQueryable<ProductListProjection> projectionsQuery,
        GetProductsRequest request,
        string language)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            var tokens = term
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(EscapeLike)
                .ToArray();

            foreach (var token in tokens)
            {
                var pattern = $"%{token}%";
                projectionsQuery = language == "ru"
                    ? projectionsQuery.Where(projection => EF.Functions.ILike(projection.SearchTextRu, pattern, @"\"))
                    : projectionsQuery.Where(projection => EF.Functions.ILike(projection.SearchTextUk, pattern, @"\"));
            }
        }

        if (request.InStock == true)
        {
            projectionsQuery = projectionsQuery.Where(projection => projection.InStock);
        }

        if (request.IsSale.HasValue)
        {
            projectionsQuery = projectionsQuery.Where(projection => projection.IsSale == request.IsSale.Value);
        }

        if (request.IsNew.HasValue)
        {
            projectionsQuery = projectionsQuery.Where(projection => projection.IsNew == request.IsNew.Value);
        }

        if (request.PriceFrom is not null)
        {
            projectionsQuery = projectionsQuery.Where(projection =>
                projection.RetailPrice != null &&
                projection.RetailPrice >= request.PriceFrom.Value);
        }

        if (request.PriceTo is not null)
        {
            projectionsQuery = projectionsQuery.Where(projection =>
                projection.RetailPrice != null &&
                projection.RetailPrice <= request.PriceTo.Value);
        }

        return projectionsQuery;
    }

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
            _ => projectionsQuery.OrderBy(projection => projection.ProductId)
        };
    }

    private static string NormalizeLanguage(string? lang)
    {
        return string.Equals(lang, "ru", StringComparison.OrdinalIgnoreCase) ? "ru" : "uk";
    }

    private static string EscapeLike(string text)
    {
        return text
            .Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal);
    }
}

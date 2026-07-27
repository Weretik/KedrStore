using Catalog.Application.Contracts.Persistence;
using Catalog.Contracts.Products.GetAdminList;
using Catalog.Contracts.Products.GetList;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.GetAdminList;

public sealed class GetAdminProductListQueryHandler(IReadCatalogDbContext catalogDbContext)
    : IQueryHandler<GetAdminProductListQuery, Result<PagedResult<List<AdminProductListRowDto>>>>
{
    private const int MaxPageSize = 100;

    public async ValueTask<Result<PagedResult<List<AdminProductListRowDto>>>> Handle(
        GetAdminProductListQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var pageNumber = query.Request.Page < 1 ? 1 : query.Request.Page;
        var pageSize = query.Request.PageSize < 1 ? 20 : Math.Min(query.Request.PageSize, MaxPageSize);
        var productsQuery = ApplySorting(
            ApplyFilters(catalogDbContext.ProductListProjections.AsNoTracking(), query.Request),
            query.Request.Sort);

        var totalRecords = await productsQuery.LongCountAsync(cancellationToken);
        var items = await JoinProductDetails(productsQuery, catalogDbContext.Products.AsNoTracking())
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var pagedInfo = new PagedInfo(
            pageNumber,
            pageSize,
            (long)Math.Ceiling(totalRecords / (double)pageSize),
            totalRecords);

        return Result.Success(new PagedResult<List<AdminProductListRowDto>>(pagedInfo, items));
    }

    private static IQueryable<AdminProductListRowDto> JoinProductDetails(
        IQueryable<ProductListProjection> projectionsQuery,
        IQueryable<Product> productsQuery)
    {
        return from projection in projectionsQuery
               join product in productsQuery on projection.ProductId equals product.Id
               select new AdminProductListRowDto
               {
                   Id = projection.ProductId.Value,
                   NameUk = projection.NameUk,
                   NameRu = projection.NameRu,
                   ProductSlug = projection.ProductSlug,
                   Photo = projection.Photo,
                   CategoryId = projection.CategoryId.Value,
                   InStock = projection.InStock,
                   IsSale = projection.IsSale,
                   IsNew = projection.IsNew,
                   ExportToSite = projection.ExportToSite,
                   Price = projection.RetailPrice,
                   Stock = product.Stock,
                   QuantityInPack = product.QuantityInPack
               };
    }

    private static IQueryable<ProductListProjection> ApplyFilters(
        IQueryable<ProductListProjection> productsQuery,
        GetAdminProductsRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var tokens = request.SearchTerm.Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(EscapeLike);

            foreach (var token in tokens)
            {
                var pattern = $"%{token}%";
                productsQuery = productsQuery.Where(projection =>
                    EF.Functions.ILike(projection.SearchTextUk, pattern, @"\") ||
                    EF.Functions.ILike(projection.SearchTextRu, pattern, @"\"));
            }
        }

        if (request.CategoryId is not null)
        {
            var categoryId = ProductCategoryId.From(request.CategoryId.Value);
            productsQuery = productsQuery.Where(projection => projection.CategoryId == categoryId);
        }
        else if (!string.IsNullOrWhiteSpace(request.CategorySlug))
        {
            productsQuery = productsQuery.Where(projection => projection.CategorySlug == request.CategorySlug.Trim());
        }

        if (request.InStock == true)
            productsQuery = productsQuery.Where(projection => projection.InStock);

        if (request.IsSale.HasValue)
            productsQuery = productsQuery.Where(projection => projection.IsSale == request.IsSale.Value);

        if (request.IsNew.HasValue)
            productsQuery = productsQuery.Where(projection => projection.IsNew == request.IsNew.Value);

        if (request.PriceFrom is not null)
            productsQuery = productsQuery.Where(projection => projection.RetailPrice != null && projection.RetailPrice >= request.PriceFrom.Value);

        if (request.PriceTo is not null)
            productsQuery = productsQuery.Where(projection => projection.RetailPrice != null && projection.RetailPrice <= request.PriceTo.Value);

        return productsQuery;
    }

    private static IQueryable<ProductListProjection> ApplySorting(
        IQueryable<ProductListProjection> productsQuery,
        ProductSort sort)
    {
        return sort switch
        {
            ProductSort.IdDesc => productsQuery.OrderByDescending(projection => projection.ProductId),
            ProductSort.NameAsc => productsQuery.OrderBy(projection => projection.NameUk).ThenBy(projection => projection.NameRu),
            ProductSort.NameDesc => productsQuery.OrderByDescending(projection => projection.NameUk).ThenByDescending(projection => projection.NameRu),
            ProductSort.PriceAsc => productsQuery.OrderBy(projection => projection.RetailPrice.HasValue ? 0 : 1).ThenBy(projection => projection.RetailPrice),
            ProductSort.PriceDesc => productsQuery.OrderBy(projection => projection.RetailPrice.HasValue ? 0 : 1).ThenByDescending(projection => projection.RetailPrice),
            _ => productsQuery.OrderBy(projection => projection.ProductId)
        };
    }

    private static string EscapeLike(string text) => text
        .Replace(@"\", @"\\", StringComparison.Ordinal)
        .Replace("%", @"\%", StringComparison.Ordinal)
        .Replace("_", @"\_", StringComparison.Ordinal);
}

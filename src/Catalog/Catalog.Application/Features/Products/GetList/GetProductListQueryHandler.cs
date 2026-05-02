using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Products.GetList.DTOs;
using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Application.Integrations.OneC.Options;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.GetList;

public class GetProductListQueryHandler(IReadCatalogDbContext catalogDbContext, IOptionsSnapshot<RootCategoryId> options)
    : IQueryHandler<GetProductListQuery, Result<PagedResult<List<ProductListRowDto>>>>
{
    private const int MaxPageSize = 100;

    public async ValueTask<Result<PagedResult<List<ProductListRowDto>>>> Handle(
        GetProductListQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var hardwareRootCategoryId = options.Value.HardwareRootCategoryId;
        var request = query.Request;

        var productQuery = catalogDbContext.Products.AsNoTracking();
        var categoryQuery = catalogDbContext.Categories.AsNoTracking();
        var priceQuery = catalogDbContext.ProductPrices.AsNoTracking();
        var translationQuery = catalogDbContext.ProductTranslations.AsNoTracking();

        productQuery = ApplyProductListFilters(productQuery, categoryQuery, request, hardwareRootCategoryId);
        var isIdSort = request.Sort is ProductSort.IdAsc or ProductSort.IdDesc;

        if (isIdSort)
        {
            productQuery = request.Sort == ProductSort.IdDesc
                ? productQuery.OrderByDescending(p => p.Id)
                : productQuery.OrderBy(p => p.Id);
        }

        var productListWithPriceQuery = JoinPricesForList(
            productQuery,
            priceQuery,
            translationQuery,
            request,
            hardwareRootCategoryId);

        var productSortListQuery = isIdSort
            ? productListWithPriceQuery
            : ApplySorting(productListWithPriceQuery, request.Sort);

        var totalRecords  = await productListWithPriceQuery.CountAsync(cancellationToken);
        var pageNumber  = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

        if (pageSize > MaxPageSize) pageSize = MaxPageSize;

        var totalPages = (long)Math.Ceiling(totalRecords / (double)pageSize);
        var skip = (pageNumber - 1) * pageSize;

        var productListQuery = productSortListQuery.Skip(skip).Take(pageSize);

        var items = await productListQuery.ToListAsync(cancellationToken);

        var pagedInfo = new PagedInfo(
            pageNumber: pageNumber,
            pageSize: pageSize,
            totalPages: totalPages,
            totalRecords: totalRecords);

        var result = new PagedResult<List<ProductListRowDto>>(pagedInfo, items);

        return Result.Success(result);

    }

    private static IQueryable<Product> ApplyProductListFilters(
        IQueryable<Product> productsQuery,
        IQueryable<ProductCategory> categoriesQuery,
        GetProductsRequest request,
        string hardwareRootCategoryId)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            var tokens = term
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(EscapeLike)
                .ToArray();

            var hasId = int.TryParse(term, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) && id > 0;

            var nameQuery = productsQuery;
            foreach (var token in tokens)
            {
                nameQuery = nameQuery.Where(x => EF.Functions.ILike(x.Name, $"%{token}%", @"\"));
            }

            if (!hasId)
            {
                productsQuery = nameQuery;
            }
            else
            {
                var productId = ProductId.From(id);
                productsQuery = productsQuery.Where(x => x.Id == productId || nameQuery.Any(p => p.Id == x.Id));
            }
        }

        if (!string.IsNullOrWhiteSpace(request.CategorySlug))
        {
            var categoryId = TryGetTrailingIntId(request.CategorySlug);
            if (categoryId is not null)
            {
                var selectedCategoryId = ProductCategoryId.From(categoryId.Value);
                var childCategoryIds = categoriesQuery
                    .Where(category => category.ParentId == selectedCategoryId)
                    .Select(category => category.Id);

                var categoryIds = categoriesQuery
                    .Where(category =>
                        category.Id == selectedCategoryId ||
                        category.ParentId == selectedCategoryId ||
                        (category.ParentId.HasValue && childCategoryIds.Contains(category.ParentId.Value)))
                    .Select(category => category.Id);

                productsQuery = productsQuery.Where(product => categoryIds.Contains(product.CategoryId));
            }
        }

        if (request.InStock == true)
        {
            productsQuery = productsQuery.Where(p =>
                (p.ProductTypeIdOneC == hardwareRootCategoryId && p.Stock > 2) ||
                (p.ProductTypeIdOneC != hardwareRootCategoryId && p.Stock > 0));
        }

        if (request.IsSale.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.IsSale == request.IsSale.Value);
        }

        if (request.IsNew.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.IsNew == request.IsNew.Value);
        }

        return productsQuery;
    }

    private static IQueryable<ProductListRowDto> JoinPricesForList(
        IQueryable<Product> productsQuery,
        IQueryable<ProductPrice> pricesQuery,
        IQueryable<ProductTranslation> translationsQuery,
        GetProductsRequest request,
        string hardwareRootCategoryId)
    {
        var language = NormalizeLanguage(request.Lang);
        var localizedTranslations = translationsQuery.Where(t => t.Language == language);
        var pricesByType = pricesQuery.Where(pr => pr.PriceTypeId == PriceTypeId.From(request.PriceTypeId));

        var productsWithTranslations = productsQuery.LeftJoin(
            localizedTranslations,
            product => product.Id,
            translation => translation.ProductId,
            (product, translation) => new { product, translation });

        var productListQuery = productsWithTranslations.LeftJoin(
            pricesByType,
            row => row.product.Id,
            price => price.ProductId,
            (row, price) => new ProductListRowDto
            {
                Id = row.product.Id.Value,
                Name = row.translation != null ? row.translation.Name : row.product.Name,
                Photo = row.product.Photo,
                ProductSlug = row.product.ProductSlug,
                CategoryId = row.product.CategoryId.Value,
                InStock = row.product.ProductTypeIdOneC == hardwareRootCategoryId
                    ? row.product.Stock > 2
                    : row.product.Stock > 0,
                IsSale = row.product.IsSale,
                IsNew = row.product.IsNew,
                Price = price != null ? price.Amount : null
            });

        if (request.PriceFrom is not null)
        {
            productListQuery = productListQuery.Where(x => x.Price != null && x.Price >= request.PriceFrom.Value);
        }

        if (request.PriceTo is not null)
        {
            productListQuery = productListQuery.Where(x => x.Price != null && x.Price <= request.PriceTo.Value);
        }

        return productListQuery;
    }

    private static IQueryable<ProductListRowDto> ApplySorting(IQueryable<ProductListRowDto> query, ProductSort sort)
    {
        return sort switch
        {
            ProductSort.PriceAsc => query.OrderBy(x => x.Price.HasValue ? 0 : 1).ThenBy(x => x.Price),
            ProductSort.PriceDesc => query.OrderBy(x => x.Price.HasValue ? 0 : 1).ThenByDescending(x => x.Price),
            ProductSort.NameAsc => query.OrderBy(x => x.Name),
            ProductSort.NameDesc => query.OrderByDescending(x => x.Name),
            _ => query.OrderBy(x => x.Name)
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

    private static int? TryGetTrailingIntId(string slug)
    {
        var lastDash = slug.LastIndexOf('-');
        if (lastDash < 0 || lastDash == slug.Length - 1)
        {
            return null;
        }

        var tail = slug[(lastDash + 1)..];
        if (int.TryParse(tail, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) && id > 0)
        {
            return id;
        }

        return null;
    }
}

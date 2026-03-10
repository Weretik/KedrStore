namespace Catalog.Application.Features.Products.GetList.Extensions;

public static class ProductListSortingExtensions
{
    public static IQueryable<ProductListRowDto> ApplySorting(this IQueryable<ProductListRowDto> query, ProductSort sort)
    {
        return sort switch
        {
            ProductSort.PriceAsc =>
                query
                    .OrderBy(x => x.Price.HasValue ? 0 : 1)
                    .ThenBy(x => x.Price),

            ProductSort.PriceDesc =>
                query
                    .OrderBy(x => x.Price.HasValue ? 0 : 1)
                    .ThenByDescending(x => x.Price),

            ProductSort.NameAsc=>
                query.OrderBy(x => x.Name),

            ProductSort.NameDesc =>
                query.OrderByDescending(x =>  x.Name),

            ProductSort.IdDesc =>
                query
                    .OrderByDescending(x => x.Id),

            _ =>
                query.OrderBy(x => x.Id),
        };
    }
}

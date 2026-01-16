namespace Catalog.Application.Features.Products.GetList;

public static class ProductListSortingExtensions
{
    public static IQueryable<ProductListRowDto> ApplySorting(this IQueryable<ProductListRowDto> query, ProductSort sort)
    {
        return sort switch
        {
            ProductSort.PriceAsc =>
                query
                    .OrderBy(x => x.Price == null)
                    .ThenBy(x => x.Price),

            ProductSort.PriceDesc =>
                query
                    .OrderBy(x => x.Price == null)
                    .ThenByDescending(x => x.Price),

            ProductSort.NameDesc =>
                query.OrderByDescending(x => x.Name),

            _ =>
                query.OrderBy(x => x.Name),
        };
    }
}

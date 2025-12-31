using Catalog.Api.Contracts;
using Catalog.Api.Contracts.Products;
using Catalog.Application.Features.Products.Queries.GetProducts;

namespace Catalog.Api.Mappers;

public static class GetProductsMapper
{
    public static GetProductsQuery ToQuery(this GetProductsRequest request)
    {
        // Pagination
        var pagination = new ProductPagination(
            CurrentPage: request.CurrentPage,
            PageSize: request.PageSize,
            All: request.All
            );

        // Filtering
        var filter = new ProductFilter(
            SearchTerm: request.SearchTerm,
            CategoryId: request.CategoryId,
            ProductTypeId: request.ProductTypeId,
            Stock: request.Stock);

        // Sorting
        var sorter = new ProductSorter(
            Key: request.Key,
            Desc: request.Desc);

        // Pricing
        var pricing = new PricingOptions(
            PriceType: request.PriceType,
            MinPrice: request.MinPrice,
            MaxPrice: request.MaxPrice);

        return new GetProductsQuery(
            Filter: filter,
            Pagination: pagination,
            Sorter: sorter,
            PricingOptions: pricing);
    }
}

using Catalog.Application.Features.Products.Queries.GetProducts;
using Catalog.Application.GetProducts;
using Catalog.Application.Shared;

namespace Presentation.Shared.States.Catalog;

public sealed class CatalogStore(IDispatcher dispatcher) : ICatalogStore
{
    // Filter
    public void SetSearchTerm(string? value)
        => dispatcher.Dispatch(new CatalogFilterAction.SetSearchTerm(value));

    public void SetCategory(int? id)
        => dispatcher.Dispatch(new CatalogFilterAction.SetCategory(id));

    public void SetStock(decimal? value)
        => dispatcher.Dispatch(new CatalogFilterAction.SetStock(value));

    public void SetProductTypeId(int? id)
        => dispatcher.Dispatch(new CatalogFilterAction.SetProductTypeId(id));

    // Sorting
    public void SetSortKey(ProductSortKey key)
        => dispatcher.Dispatch(new CatalogSortingAction.SetSortKey(key));

    public void SetSortDesc(bool desc)
        => dispatcher.Dispatch(new CatalogSortingAction.SetSortDesc(desc));

    // Pagination
    public void SetPageNumber(int page)
        => dispatcher.Dispatch(new CatalogPaginationAction.SetPageNumber(page));

    public void SetPageSize(int size)
        => dispatcher.Dispatch(new CatalogPaginationAction.SetPageSize(size));

    public void SetAllPageSize(bool all)
        => dispatcher.Dispatch(new CatalogPaginationAction.SetAllPageSize(all));

    // Pricing
    public void SetPriceId(string priceTypeId)
        => dispatcher.Dispatch(new CatalogPricingAction.SetPriceType(priceTypeId));

    public void SetPriceRange(decimal? min, decimal? max)
        => dispatcher.Dispatch(new CatalogPricingAction.SetPriceRange(min, max));

    // Data
    public void Load()
        => dispatcher.Dispatch(new CatalogLoadAction.Load());

    public void LoadFailure(string error)
        => dispatcher.Dispatch(new CatalogLoadAction.LoadFailure(error));

    public void LoadSuccess(GetProductsQueryResult<ProductDto> queryResult)
        => dispatcher.Dispatch(new CatalogLoadAction.LoadSuccess(queryResult));
    public void Reset()
        => dispatcher.Dispatch(new CatalogLoadAction.Reset());
}

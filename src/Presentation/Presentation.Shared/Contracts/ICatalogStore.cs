using Application.Catalog.GetProducts;
using Application.Catalog.Shared;

namespace Presentation.Shared;

public interface ICatalogStore
{
    // Filter
    void SetSearchTerm(string? value);
    void SetCategory(ProductCategoryId? id);
    void SetStock(decimal? value);

    // Sorting
    void SetSortKey(ProductSortKey key);
    void SetSortDesc(bool desc);

    // Pagination
    void SetPageNumber(int page);
    void SetPageSize(int size);
    void SetAllPageSize(bool all);

    // Pricing
    void SetPriceId(string priceType);
    void SetPriceRange(decimal? min, decimal? max);

    // Data
    void Load();
    void LoadFailure(string error);
    void LoadSuccess(GetProductsQueryResult<ProductDto> queryResult);
    void Reset();
}

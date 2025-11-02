using Application.Catalog.GetProducts;
using Application.Catalog.Shared;

namespace Presentation.Shared.States.Catalog;

public static class CatalogPaginationAction
{
    public sealed record SetPagination(ProductPagination Pagination);
    public sealed record SetPageNumber(int PageNumber);
    public sealed record SetPageSize(int PageSize);
    public sealed record SetAllPage(bool All);
}

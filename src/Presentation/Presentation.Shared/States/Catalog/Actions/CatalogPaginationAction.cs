using Catalog.Application.Features.Products.Queries.GetProducts;
using Catalog.Application.GetProducts;

namespace Presentation.Shared.States.Catalog;

public static class CatalogPaginationAction
{
    public sealed record SetPagination(ProductPagination Pagination);
    public sealed record SetPageNumber(int PageNumber);
    public sealed record SetPageSize(int PageSize);
    public sealed record SetAllPageSize(bool All);
}

namespace Catalog.Application.Features.Products.Queries.GetProducts;

public sealed record ProductPagination(int CurrentPage = 1, int PageSize = 12, bool All = false);

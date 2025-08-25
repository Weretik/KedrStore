namespace Presentation.Shared.States.Catalog;

public sealed record CatalogParams(
    int PageNumber = 1,
    int PageSize = 12,
    int? CategoryId = null,
    string? SearchTerm = null,
    string? Sort = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? Manufacturer = null);

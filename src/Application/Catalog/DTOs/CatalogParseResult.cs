namespace Application.Catalog.DTOs;

public sealed record CatalogParseResult(IReadOnlyList<CategoryDto> Categories, IReadOnlyList<ProductDto> Products);

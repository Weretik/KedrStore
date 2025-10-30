using Application.Catalog.Shared;

namespace Application.Catalog.ImportCatalogFromXml;

public sealed record CatalogParseResult(IReadOnlyList<ImportCategoryDto> Categories, IReadOnlyList<ProductDto> Products);

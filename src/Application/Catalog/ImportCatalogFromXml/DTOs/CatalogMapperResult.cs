using Application.Catalog.Shared;

namespace Application.Catalog.ImportCatalogFromXml;

public sealed record CatalogMapperResult(IReadOnlyList<ImportCategoryDto> Categories, IReadOnlyList<ProductDto> Products);

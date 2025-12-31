using Catalog.Application.Features.Shared;
using Catalog.Application.Shared;

namespace Catalog.Application.Features.Import.Queries.ImportCatalogFromXml;

public sealed record CatalogMapperResult(IReadOnlyList<ImportCategoryDto> Categories, IReadOnlyList<ProductDto> Products);

using Catalog.Application.Shared;

namespace Catalog.Application.Features.Import.Queries.ImportCatalogFromXml;

public sealed record ImportCatalogFromXml(UploadedFile File, int ProductTypeId);

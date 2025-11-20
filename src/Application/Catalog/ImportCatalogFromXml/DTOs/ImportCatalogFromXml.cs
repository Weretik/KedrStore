using Application.Catalog.Shared;

namespace Application.Catalog.ImportCatalogFromXml;

public sealed record ImportCatalogFromXml(UploadedFile File, int ProductTypeId);

namespace Application.Catalog.ImportCatalogFromXml;

public sealed  record ImportCatalogFromXml(
    string FileName,
    string? ContentType,
    long FileSize,
    Stream Content,
    int ProductType)
    : ICatalogFromXml;

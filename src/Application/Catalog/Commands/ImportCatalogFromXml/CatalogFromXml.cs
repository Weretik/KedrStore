namespace Application.Catalog.Commands.ImportCatalogFromXml;

public sealed  record CatalogFromXml(string FileName, string? ContentType, long FileSize, Stream Content, int productType)
    : ICatalogFromXml;

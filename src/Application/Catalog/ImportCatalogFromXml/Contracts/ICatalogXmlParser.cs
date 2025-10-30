namespace Application.Catalog.ImportCatalogFromXml;

public interface ICatalogXmlParser
{
    Task<CatalogParseResult> ParseAsync(Stream xml, int productTypeId, CancellationToken cancellationToken);
}

namespace Application.Catalog.Interfaces;

public interface ICatalogXmlParser
{
    Task<CatalogParseResult> ParseAsync(Stream xml, CancellationToken ct);
}

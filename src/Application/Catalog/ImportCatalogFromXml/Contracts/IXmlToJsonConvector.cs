namespace Application.Catalog.ImportCatalogFromXml;

public interface IXmlToJsonConvector
{
    Task<Stream> CreateJsonStreamAsync(Stream xmlStream, CancellationToken cancellationToken);
}

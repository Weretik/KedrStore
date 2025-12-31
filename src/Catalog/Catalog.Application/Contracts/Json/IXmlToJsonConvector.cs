namespace Catalog.Application.Features.Import.Queries.ImportCatalogFromXml;

public interface IXmlToJsonConvector
{
    Task<Stream> CreateJsonStreamAsync(Stream xmlStream, CancellationToken cancellationToken);
}

namespace Catalog.Application.Contracts.Json;

public interface IXmlToJsonConvector
{
    Task<Stream> CreateJsonStreamAsync(Stream xmlStream, CancellationToken cancellationToken);
}

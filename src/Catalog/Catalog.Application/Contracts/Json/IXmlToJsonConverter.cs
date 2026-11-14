namespace Catalog.Application.Contracts.Json;

public interface IXmlToJsonConverter
{
    Task<Stream> CreateJsonStreamAsync(Stream xmlStream, CancellationToken cancellationToken);
}

namespace Catalog.Application.Abstractions.Integrations;

public interface IXmlToJsonConverter
{
    Task<Stream> CreateJsonStreamAsync(Stream xmlStream, CancellationToken cancellationToken);
}

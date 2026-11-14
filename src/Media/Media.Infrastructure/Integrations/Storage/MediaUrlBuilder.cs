using Media.Application.Abstractions.Storage;
using Microsoft.Extensions.Options;

namespace Media.Infrastructure.Integrations.Storage;

internal sealed class MediaUrlBuilder(IOptions<MediaStorageOptions> options) : IMediaUrlBuilder
{
    private readonly string _baseUrl = options.Value.PublicBaseUrl.TrimEnd('/');

    public string Build(string storageKey)
    {
        var path = string.Join('/', storageKey.TrimStart('/').Split('/').Select(Uri.EscapeDataString));
        return $"{_baseUrl}/{path}";
    }
}

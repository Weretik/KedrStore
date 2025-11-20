using System.Collections.Concurrent;
using Infrastructure.Common.Contracts;

namespace Infrastructure.Common.Services;

public sealed class ImageResolver(HttpClient client) : IImageResolver
{
    private readonly string _fallback =
        "https://raw.githubusercontent.com/Kedr-Class/images/main/furniture/00000000.jpg";

    private static readonly ConcurrentDictionary<int, string> _cache = new();

    public async Task<string> ResolveAsync(int id, CancellationToken ct)
    {
        if (_cache.TryGetValue(id, out var cached))
            return cached;

        var url = $"https://raw.githubusercontent.com/Kedr-Class/images/refs/heads/main/furniture/{id}.jpg";

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Head, url);
            var resp = await client.SendAsync(req, ct);

            var finalUrl = resp.IsSuccessStatusCode ? url : _fallback;
            _cache[id] = finalUrl;
            return finalUrl;
        }
        catch
        {
            _cache[id] = _fallback;
            return _fallback;
        }
    }
}

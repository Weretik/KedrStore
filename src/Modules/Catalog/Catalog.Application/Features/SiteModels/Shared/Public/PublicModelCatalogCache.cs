namespace Catalog.Application.Features.SiteModels.Shared.Public;

/// <summary>
/// Keeps the public model catalogue for <see cref="Lifetime"/>, so the public site's model pages read the whole
/// catalogue once per period instead of once per request (database rules 1.4; audit 2026-11-15 finding E). Requests
/// that miss at the same time wait for one load. A change in the admin reaches the site within <see cref="Lifetime"/>.
/// The catalogue is read-only, so one instance serves every request of this process.
/// </summary>
public sealed class PublicModelCatalogCache(TimeProvider clock)
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(2);

    private readonly SemaphoreSlim _gate = new(1, 1);
    private volatile Entry? _entry;

    public async Task<PublicModelCatalog> GetAsync(Func<CancellationToken, Task<PublicModelCatalog>> load,
        CancellationToken cancellationToken)
    {
        if (Fresh() is { } catalog)
            return catalog;

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (Fresh() is { } loaded)
                return loaded;
            var read = await load(cancellationToken);
            _entry = new Entry(read, clock.GetUtcNow() + Lifetime);
            return read;
        }
        finally
        {
            _gate.Release();
        }
    }

    private PublicModelCatalog? Fresh()
        => _entry is { } entry && entry.Until > clock.GetUtcNow() ? entry.Catalog : null;

    private sealed record Entry(PublicModelCatalog Catalog, DateTimeOffset Until);
}

using BuildingBlocks.Application.Caching;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Infrastructure.Caching;

public static class ReadCacheRegistration
{
    /// <summary>
    /// Largest answer kept; a bigger one is returned but not cached (HybridCache logs it). The site models state of the
    /// whole catalog is the largest entry (a few MB for a few thousand products with their attribute values).
    /// </summary>
    public const int MaximumPayloadBytes = 16 * 1024 * 1024;

    /// <summary>
    /// Registers <see cref="HybridCache"/> in memory only and <see cref="IReadCache"/> over it (ADR-0016). To add a
    /// shared L2 later, register an <c>IDistributedCache</c> (for example <c>AddStackExchangeRedisCache</c>) in the host:
    /// HybridCache uses it on its own. Safe to call more than once.
    /// </summary>
    public static IServiceCollection AddReadCache(this IServiceCollection services)
    {
        services.AddHybridCache(options => options.MaximumPayloadBytes = MaximumPayloadBytes);
        services.TryAddSingleton<IReadCache, HybridReadCache>();
        return services;
    }
}

using BuildingBlocks.Application.Caching;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Caching;

public static class ReadCacheRegistration
{
    /// <summary>Largest answer kept; a bigger one is returned but not cached.</summary>
    public const int MaximumPayloadBytes = 4 * 1024 * 1024;

    /// <summary>
    /// Registers <see cref="HybridCache"/> in memory only and <see cref="IReadCache"/> over it (ADR-0016). To add a
    /// shared L2 later, register an <c>IDistributedCache</c> (for example <c>AddStackExchangeRedisCache</c>) in the host:
    /// HybridCache uses it on its own.
    /// </summary>
    public static IServiceCollection AddReadCache(this IServiceCollection services)
    {
        services.AddHybridCache(options => options.MaximumPayloadBytes = MaximumPayloadBytes);
        services.AddSingleton<IReadCache, HybridReadCache>();
        return services;
    }
}

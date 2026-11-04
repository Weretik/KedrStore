using Media.Application.Contracts.Images;
using Media.Application.Contracts.Storage;
using Media.Application.Jobs.CdnSync;
using Media.Infrastructure.Images;
using Media.Infrastructure.Repositories;
using Media.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Media.Infrastructure.DependencyInjection;

public static class MediaInfrastructureExtensions
{
    public static IServiceCollection AddMediaInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");

        services.AddDbContext<MediaDbContext>(options => MediaDbContext.Configure(options, connectionString));
        services.AddScoped<IReadMediaDbContext>(sp => sp.GetRequiredService<MediaDbContext>());
        services.AddScoped(typeof(IMediaRepository<>), typeof(MediaEfRepository<>));
        services.AddSingleton<IImageInspector, SkiaImageInspector>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IDatabaseMigrator, DbMigrator<MediaDbContext>>();

        var storageSection = configuration.GetSection(MediaStorageOptions.SectionName);
        services.Configure<MediaStorageOptions>(storageSection);
        services.AddSingleton<IMediaUrlBuilder, MediaUrlBuilder>();
        // Without CDN credentials (local runs, tests) writes answer 503 instead of failing at startup.
        var storage = storageSection.Get<MediaStorageOptions>() ?? new MediaStorageOptions();
        if (storage.IsConfigured)
            services.AddSingleton<IMediaStorage, FtpMediaStorage>();
        else
            services.AddSingleton<IMediaStorage, UnconfiguredMediaStorage>();

        services.Configure<MediaCdnSyncOptions>(configuration.GetSection(MediaCdnSyncOptions.SectionName));
        services.AddScoped<SyncCdnMediaJob>();
        return services;
    }
}

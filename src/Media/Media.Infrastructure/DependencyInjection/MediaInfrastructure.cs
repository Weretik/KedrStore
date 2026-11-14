using Media.Application.Abstractions.Images;
using Media.Application.Abstractions.Storage;
using Media.Application.Features.Files.Shared;
using Media.Application.Features.Files.Jobs.SyncCdnMedia;
using Media.Contracts.Files;
using Media.Contracts.Folders;
using Media.Infrastructure.Contracts.Files;
using Media.Infrastructure.Contracts.Folders;
using Media.Infrastructure.Integrations.Images;
using Media.Infrastructure.Contracts.Files;
using Media.Infrastructure.Persistence;
using Media.Infrastructure.Integrations.Storage;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Media.Infrastructure.DependencyInjection;

public static class MediaInfrastructure
{
    public static IServiceCollection AddMediaInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");

        services.AddDbContext<MediaDbContext>(options => MediaDbContext.Configure(options, connectionString));
        services.AddScoped<IReadMediaDbContext>(sp => sp.GetRequiredService<MediaDbContext>());
        services.AddScoped(typeof(IMediaRepository<>), typeof(MediaEfRepository<>));
        services.AddScoped<IMediaFileLookup, MediaFileLookup>();
        services.AddSingleton<IImageInspector, SkiaImageInspector>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IDatabaseMigrator, DbMigrator<MediaDbContext>>();

        var storageSection = configuration.GetSection(MediaStorageOptions.SectionName);
        services.Configure<MediaStorageOptions>(storageSection);
        services.AddSingleton<IMediaUrlBuilder, MediaUrlBuilder>();
        services.AddScoped<IMediaFileDirectory, MediaFileDirectory>();
        services.AddScoped<IMediaFolderArranger, MediaFolderArranger>();
        services.AddScoped<IMediaFileUploader, MediaFileUploader>();
        services.AddScoped<MediaFileChangeNotifier>();
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

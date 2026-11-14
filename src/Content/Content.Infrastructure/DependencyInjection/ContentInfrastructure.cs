using Content.Application.Abstractions.Integrations;
using Content.Application.Abstractions.Media;
using Content.Infrastructure.Integrations.Youtube;
using Content.Infrastructure.Adapters.Media;
using Content.Infrastructure.Integrations.Html;
using Content.Infrastructure.Integrations.Images;
using Content.Infrastructure.Integrations.Site;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Content.Infrastructure.DependencyInjection;

public static class ContentInfrastructure
{
    public static IServiceCollection AddContentInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");

        services.AddDbContext<ContentDbContext>(options => ContentDbContext.Configure(options, connectionString));
        services.AddScoped<IReadContentDbContext>(sp => sp.GetRequiredService<ContentDbContext>());
        services.AddScoped(typeof(IContentRepository<>), typeof(ContentEfRepository<>));
        services.AddSingleton<IArticleHtmlSanitizer, ArticleHtmlSanitizer>();
        services.AddSingleton<ICoverImageProcessor, CoverImageProcessor>();
        services.AddScoped<IMediaImageReader, MediaImageReader>();
        services.AddScoped<IMediaImageWriter, MediaImageWriter>();
        services.AddHttpClient<IYoutubeVideoInfo, YoutubeOEmbedClient>(client => client.Timeout = TimeSpan.FromSeconds(5));
        services.Configure<SiteRevalidationOptions>(configuration.GetSection(SiteRevalidationOptions.SectionName));
        services.AddHttpClient<ISiteCacheRefresher, SiteCacheRefresher>(client => client.Timeout = TimeSpan.FromSeconds(10));
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IDatabaseMigrator, DbMigrator<ContentDbContext>>();
        return services;
    }
}

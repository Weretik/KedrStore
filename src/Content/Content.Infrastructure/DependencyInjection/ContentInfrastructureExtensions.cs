using Content.Application.Contracts.Html;
using Content.Application.Contracts.Images;
using Content.Infrastructure.Html;
using Content.Infrastructure.Images;
using Content.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Content.Infrastructure.DependencyInjection;

public static class ContentInfrastructureExtensions
{
    public static IServiceCollection AddContentInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");

        services.AddDbContext<ContentDbContext>(options => ContentDbContext.Configure(options, connectionString));
        services.AddScoped<IReadContentDbContext>(sp => sp.GetRequiredService<ContentDbContext>());
        services.AddScoped(typeof(IContentRepository<>), typeof(ContentEfRepository<>));
        services.AddSingleton<IArticleHtmlSanitizer, ArticleHtmlSanitizer>();
        services.AddSingleton<ICoverImageProcessor, CoverImageProcessor>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IDatabaseMigrator, DbMigrator<ContentDbContext>>();
        return services;
    }
}

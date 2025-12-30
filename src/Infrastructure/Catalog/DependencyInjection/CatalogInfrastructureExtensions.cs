using Application.Catalog.Shared;
using Application.Catalog.CreateQuickOrder;
using Infrastructure.Catalog.Repositories;
using Infrastructure.Catalog.Notifications;
using Microsoft.Extensions.Options;
using BuildingBlocks.Infrastructure.Services;

namespace Infrastructure.Catalog.DependencyInjection;

public static class CatalogInfrastructureExtensions
{
    public static IServiceCollection AddCatalogInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");

        services.AddDbContext<CatalogDbContext>(options => options.UseNpgsql(connectionString));
        services.AddDbContextFactory<CatalogDbContext>(options => options.UseNpgsql(connectionString),
            lifetime: ServiceLifetime.Scoped);

        services.AddScoped(typeof(ICatalogRepository<>), typeof(CatalogEfRepository<>));
        services.AddScoped(typeof(ICatalogReadRepository<>), typeof(CatalogReadEfRepository<>));

        services.AddScoped<IDatabaseMigrator, DbMigrator<CatalogDbContext>>();

        services.AddScoped<ITelegramNotifier, TelegramBotNotifier>();

        return services;
    }
}

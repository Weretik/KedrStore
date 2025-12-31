using Catalog.Application.Features.Orders.Commands.CreateQuickOrder;
using Catalog.Application.Persistance;
using Catalog.Application.Shared;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Notifications;
using Catalog.Infrastructure.Repositories;

namespace Catalog.Infrastructure.DependencyInjection;

public static class CatalogInfrastructureDependencyInjection
{
    public static IServiceCollection AddCatalogInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");

        services.AddDbContext<CatalogDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<CatalogDbContext>());
        services.AddDbContextFactory<CatalogDbContext>(options => options.UseNpgsql(connectionString),
            lifetime: ServiceLifetime.Scoped);

        services.AddScoped(typeof(ICatalogRepository<>), typeof(CatalogEfRepository<>));
        services.AddScoped(typeof(ICatalogReadRepository<>), typeof(CatalogReadEfRepository<>));

        services.AddScoped<IDatabaseMigrator, DbMigrator<CatalogDbContext>>();

        services.AddScoped<ITelegramNotifier, TelegramBotNotifier>();

        return services;
    }
}

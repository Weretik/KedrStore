using Catalog.Application.Features.Orders.Commands.CreateQuickOrder;
using Catalog.Application.Persistence;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Notifications;
using Catalog.Infrastructure.Repositories;

namespace Catalog.Infrastructure.DependencyInjection;

public static class CatalogInfrastructureExtensions
{
    public static IServiceCollection AddCatalogInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");;

        services.AddDbContext<CatalogDbContext>(options => options.UseNpgsql(connectionString));
        //services.AddScoped<DbContext>(sp => sp.GetRequiredService<CatalogDbContext>());
        services.AddScoped<IReadCatalogDbContext>(sp => sp.GetRequiredService<CatalogDbContext>());

        services.AddScoped(typeof(ICatalogRepository<>), typeof(CatalogEfRepository<>));
        services.AddScoped(typeof(ICatalogReadRepository<>), typeof(CatalogReadEfRepository<>));

        services.AddScoped<IDatabaseMigrator, DbMigrator<CatalogDbContext>>();

        services.AddScoped<ITelegramNotifier, TelegramBotNotifier>();

        return services;
    }
}

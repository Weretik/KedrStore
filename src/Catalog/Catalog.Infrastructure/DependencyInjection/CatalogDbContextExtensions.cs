using Catalog.Application.Features.Category.Stats;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Contracts.Projections;
using Catalog.Application.Features.Category;
using Catalog.Application.Slugs;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Projections;
using Catalog.Infrastructure.Repositories;
using Catalog.Infrastructure.Products;

namespace Catalog.Infrastructure.DependencyInjection;

public static class CatalogDbContextExtensions
{
    public static IServiceCollection AddCatalogDbContextServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");

        services.AddDbContext<CatalogDbContext>(
            options => options.UseNpgsql(connectionString),
            optionsLifetime: ServiceLifetime.Singleton);
        services.AddDbContextFactory<CatalogDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IReadCatalogDbContext>(sp => sp.GetRequiredService<CatalogDbContext>());

        services.AddScoped(typeof(ICatalogRepository<>), typeof(CatalogEfRepository<>));
        services.AddScoped(typeof(ICatalogReadRepository<>), typeof(CatalogReadEfRepository<>));
        services.AddScoped<IProductListProjectionRebuilder, ProductListProjectionRebuilder>();
        services.AddScoped<IProductPhotoCheckStore, ProductPhotoCheckStore>();
        services.AddScoped<ISlugService, SlugService>();
        services.AddScoped<ICategoryStatsReader, CategoryStatsReader>();

        services.AddScoped<IDatabaseMigrator, DbMigrator<CatalogDbContext>>();

        return services;
    }
}

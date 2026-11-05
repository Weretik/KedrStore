using Catalog.Application.Features.Attributes.Common;
using Catalog.Application.Features.ProductAttributes.Common;
using Catalog.Application.Features.Groups.Common;
using Catalog.Application.Groups;
using Catalog.Application.Features.Category.AdminGetList;
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
        services.AddScoped<ISlugService, SlugService>();
        services.AddScoped<ProductGroupBuilder>();
        services.AddScoped<ProductGroupPins>();
        services.AddScoped<ProductGroupReader>();
        services.AddScoped<ProductAttributesReader>();
        services.AddScoped<CatalogAttributeReader>();
        services.AddScoped<UnresolvedCodeReader>();
        services.AddScoped<CategoryFilterReader>();
        services.AddScoped<ICategoryStatsReader, CategoryStatsReader>();
        services.AddScoped<IAdminCategoryInsightsReader, AdminCategoryInsightsReader>();

        services.AddScoped<IDatabaseMigrator, DbMigrator<CatalogDbContext>>();

        return services;
    }
}

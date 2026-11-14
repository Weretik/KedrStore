using Catalog.Application.Features.SiteModels.Shared.Public;
using Catalog.Application.Features.SiteModels.Shared;
using Catalog.Application.Features.Attributes.Shared;
using Catalog.Application.Features.ProductAttributes.Shared;
using Catalog.Application.Features.Groups.Shared;
using Catalog.Application.Features.Categories.Queries.GetAdminCategories;
using Catalog.Application.Features.Categories.Shared;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Slugs.Shared;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Products;

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
        services.AddScoped<SiteModelStateReader>();
        services.AddScoped<SiteModelReader>();
        services.AddScoped<PublicModelCatalogReader>();
        services.AddScoped<SiteModelChanges>();
        services.AddScoped<UnresolvedCodeReader>();
        services.AddScoped<CodeMeaningReader>();
        services.AddScoped<Catalog.Application.Features.Attributes.Shared.Import.RecognizedProductWriter>();
        services.AddScoped<Catalog.Application.Features.Attributes.Jobs.RecognizeCatalogAttributesJob>();
        services.AddScoped<CategoryFilterReader>();
        services.AddScoped<ICategoryStatsReader, CategoryStatsReader>();
        services.AddScoped<IAdminCategoryInsightsReader, AdminCategoryInsightsReader>();

        services.AddScoped<IDatabaseMigrator, DbMigrator<CatalogDbContext>>();

        return services;
    }
}

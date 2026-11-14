using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Catalog.Api.Endpoints.Attributes;
using Catalog.Api.Endpoints.Categories;
using Catalog.Api.Endpoints.Groups;
using Catalog.Api.Endpoints.MobileSync;
using Catalog.Api.Endpoints.Overview;
using Catalog.Api.Endpoints.ProductAttributes;
using Catalog.Api.Endpoints.Products;
using Catalog.Api.Endpoints.SiteModels;
using Catalog.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.DependencyInjection;

/// <summary>Registers the Catalog module: Application validators, Infrastructure, Contracts implementations, and endpoints.</summary>
public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCatalogInfrastructure(configuration);
        services.AddValidatorsFromAssembly(typeof(Catalog.Application.CatalogApplicationAssemblyMarker).Assembly, includeInternalTypes: true);

        return services;
    }

    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapAttributesEndpoints();
        endpoints.MapCategoriesEndpoints();
        endpoints.MapGroupsEndpoints();
        endpoints.MapMobileSyncEndpoints();
        endpoints.MapOverviewEndpoints();
        endpoints.MapProductAttributesEndpoints();
        endpoints.MapProductsEndpoints();
        endpoints.MapSiteModelsEndpoints();
        return endpoints;
    }
}

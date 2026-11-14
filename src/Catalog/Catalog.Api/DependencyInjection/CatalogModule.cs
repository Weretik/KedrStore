using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Catalog.Infrastructure.DependencyInjection;
namespace Catalog.Api.DependencyInjection;

/// <summary>Registers the Catalog module: Application validators, Infrastructure, Contracts implementations, and controllers.</summary>
public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCatalogInfrastructure(configuration);
        services.AddValidatorsFromAssembly(typeof(Catalog.Application.CatalogApplicationAssemblyMarker).Assembly, includeInternalTypes: true);
        services.AddControllers().AddApplicationPart(typeof(CatalogApiAssemblyMarker).Assembly);

        return services;
    }
}

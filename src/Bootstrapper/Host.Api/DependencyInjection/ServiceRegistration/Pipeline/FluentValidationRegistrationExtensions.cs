using Catalog.Application;
using Content.Application;
using FluentValidation;
using Identity.Application;
using Media.Application;
using Sales.Application;

namespace Host.Api.DependencyInjection.ServiceRegistration.Pipeline;

public static class FluentValidationRegistrationExtensions
{
    public static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            typeof(CatalogApplicationAssemblyMarker).Assembly,
            includeInternalTypes: true);

        services.AddValidatorsFromAssembly(
            typeof(IdentityApplicationAssemblyMarker).Assembly,
            includeInternalTypes: true);

        services.AddValidatorsFromAssembly(
            typeof(SalesApplicationAssemblyMarker).Assembly,
            includeInternalTypes: true);

        services.AddValidatorsFromAssembly(
            typeof(ContentApplicationAssemblyMarker).Assembly,
            includeInternalTypes: true);

        services.AddValidatorsFromAssembly(
            typeof(MediaApplicationAssemblyMarker).Assembly,
            includeInternalTypes: true);

        return services;
    }
}

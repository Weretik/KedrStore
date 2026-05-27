using Catalog.Application;
using FluentValidation;
using Identity.Application;
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

        return services;
    }
}

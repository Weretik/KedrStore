using FluentValidation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sales.Api.Endpoints.Catalog;
using Sales.Api.Endpoints.Customers;
using Sales.Api.Endpoints.MobileSync;
using Sales.Api.Endpoints.Orders;
using Sales.Api.Endpoints.OrderWorkspace;
using Sales.Infrastructure.DependencyInjection;

namespace Sales.Api.DependencyInjection;

/// <summary>Registers the Sales module: Application validators, Infrastructure, Contracts implementations, and endpoints.</summary>
public static class SalesModule
{
    public static IServiceCollection AddSalesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSalesInfrastructure(configuration);
        services.AddValidatorsFromAssembly(typeof(Sales.Application.SalesApplicationAssemblyMarker).Assembly, includeInternalTypes: true);

        return services;
    }

    public static IEndpointRouteBuilder MapSalesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapCatalogEndpoints();
        endpoints.MapMobileSyncEndpoints();
        endpoints.MapCustomersEndpoints();
        endpoints.MapOrdersEndpoints();
        endpoints.MapOrderWorkspaceEndpoints();
        return endpoints;
    }
}

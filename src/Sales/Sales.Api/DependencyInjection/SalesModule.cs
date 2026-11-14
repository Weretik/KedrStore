using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Sales.Infrastructure.DependencyInjection;
namespace Sales.Api.DependencyInjection;

/// <summary>Registers the Sales module: Application validators, Infrastructure, Contracts implementations, and controllers.</summary>
public static class SalesModule
{
    public static IServiceCollection AddSalesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSalesInfrastructureServices(configuration);
        services.AddValidatorsFromAssembly(typeof(Sales.Application.SalesApplicationAssemblyMarker).Assembly, includeInternalTypes: true);
        services.AddControllers().AddApplicationPart(typeof(SalesApiAssemblyMarker).Assembly);

        return services;
    }
}

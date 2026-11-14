using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Identity.Infrastructure.DependencyInjection;
namespace Identity.Api.DependencyInjection;

/// <summary>Registers the Identity module: Application validators, Infrastructure, Contracts implementations, and controllers.</summary>
public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityOptions(configuration);
        services.AddIdentityInfrastructureServices(configuration);
        services.AddValidatorsFromAssembly(typeof(Identity.Application.IdentityApplicationAssemblyMarker).Assembly, includeInternalTypes: true);
        services.AddControllers().AddApplicationPart(typeof(IdentityApiAssemblyMarker).Assembly);

        return services;
    }
}

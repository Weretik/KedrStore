using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Identity.Api.Endpoints.Auth;
using Identity.Api.Endpoints.Profile;
using Identity.Api.Endpoints.Users;
using Identity.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Routing;

namespace Identity.Api.DependencyInjection;

/// <summary>Registers the Identity module: Application validators, Infrastructure, Contracts implementations, and endpoints.</summary>
public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityOptions(configuration);
        services.AddIdentityInfrastructure(configuration);
        services.AddValidatorsFromAssembly(typeof(Identity.Application.IdentityApplicationAssemblyMarker).Assembly, includeInternalTypes: true);

        return services;
    }

    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapAuthEndpoints();
        endpoints.MapProfileEndpoints();
        endpoints.MapUsersEndpoints();
        return endpoints;
    }
}

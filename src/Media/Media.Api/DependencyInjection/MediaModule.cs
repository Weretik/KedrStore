using FluentValidation;
using Media.Api.Endpoints.Files;
using Media.Api.Endpoints.Folders;
using Media.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Media.Api.DependencyInjection;

/// <summary>Registers the Media module: Application validators, Infrastructure, Contracts implementations, and endpoints.</summary>
public static class MediaModule
{
    public static IServiceCollection AddMediaModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediaInfrastructure(configuration);
        services.AddValidatorsFromAssembly(typeof(Media.Application.MediaApplicationAssemblyMarker).Assembly, includeInternalTypes: true);

        return services;
    }

    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapFilesEndpoints();
        endpoints.MapFoldersEndpoints();
        return endpoints;
    }
}

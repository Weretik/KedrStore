using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Media.Infrastructure.DependencyInjection;
namespace Media.Api.DependencyInjection;

/// <summary>Registers the Media module: Application validators, Infrastructure, Contracts implementations, and controllers.</summary>
public static class MediaModule
{
    public static IServiceCollection AddMediaModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediaInfrastructureServices(configuration);
        services.AddValidatorsFromAssembly(typeof(Media.Application.MediaApplicationAssemblyMarker).Assembly, includeInternalTypes: true);
        services.AddControllers().AddApplicationPart(typeof(MediaApiAssemblyMarker).Assembly);

        return services;
    }
}

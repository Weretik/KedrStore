using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Content.Infrastructure.DependencyInjection;
namespace Content.Api.DependencyInjection;

/// <summary>Registers the Content module: Application validators, Infrastructure, Contracts implementations, and controllers.</summary>
public static class ContentModule
{
    public static IServiceCollection AddContentModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddContentInfrastructure(configuration);
        services.AddValidatorsFromAssembly(typeof(Content.Application.ContentApplicationAssemblyMarker).Assembly, includeInternalTypes: true);
        services.AddControllers().AddApplicationPart(typeof(ContentApiAssemblyMarker).Assembly);

        return services;
    }
}

using Host.Api.Http.OpenApi;
using BuildingBlocks.Api.Results;
using BuildingBlocks.Api.Errors;
using BuildingBlocks.Infrastructure.Persistence;
namespace Host.Api.Http;

public static class ApiRegistrationsExtensions
{
    public static IServiceCollection AddApiRegistrations(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddApiDocument();

        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>(DatabaseHealthCheck.Name, tags: [DatabaseHealthCheck.ReadyTag]);
        services.AddApiProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddMobileSyncCompression();
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}

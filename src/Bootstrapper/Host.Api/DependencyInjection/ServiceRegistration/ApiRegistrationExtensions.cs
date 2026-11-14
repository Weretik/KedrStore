using BuildingBlocks.Api.Results;
using BuildingBlocks.Api.Errors;
using Host.Api.DependencyInjection.ServiceRegistration.Web;

namespace Host.Api.DependencyInjection.ServiceRegistration;

public static class ApiRegistrationsExtensions
{
    public static IServiceCollection AddApiRegistrations(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerDocumentation();
        services.AddOpenApi();

        services.AddHealthChecks();
        services.AddApiProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddMobileSyncCompression();
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}

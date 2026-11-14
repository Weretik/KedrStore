using BuildingBlocks.Api.Results;
using BuildingBlocks.Api.Errors;
namespace Host.Api.Http;

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

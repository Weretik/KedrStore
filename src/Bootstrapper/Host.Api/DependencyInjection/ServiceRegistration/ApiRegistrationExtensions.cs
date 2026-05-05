using Host.Api.DependencyInjection.ServiceRegistration.Web;

namespace Host.Api.DependencyInjection.ServiceRegistration;

public static class ApiRegistrationsExtensions
{
    public static IServiceCollection AddApiRegistrations(this IServiceCollection services)
    {
        services.AddModuleControllers();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddOpenApi();

        services.AddHealthChecks();
        services.AddProblemDetails();
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}

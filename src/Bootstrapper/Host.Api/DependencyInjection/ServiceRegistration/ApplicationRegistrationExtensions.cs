using Host.Api.DependencyInjection.ServiceRegistration.Pipeline;

namespace Host.Api.DependencyInjection.ServiceRegistration;

public static class ApplicationRegistrationsExtensions
{
    public static IServiceCollection AddApplicationRegistrations(this IServiceCollection services)
    {
        services.AddMediatorPipeline();
        services.AddFluentValidation();

        return services;
    }
}

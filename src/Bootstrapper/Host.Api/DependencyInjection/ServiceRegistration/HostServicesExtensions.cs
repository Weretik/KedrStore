namespace Host.Api.DependencyInjection.ServiceRegistration;

public static class HostServicesExtensions
{
    public static IServiceCollection AddHostServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddModuleRegistrations(configuration);
        services.AddBackgroundJobRegistrations();
        services.AddApplicationRegistrations();
        services.AddApiRegistrations();
        services.AddSecurityRegistrations(configuration);

        return services;
    }
}

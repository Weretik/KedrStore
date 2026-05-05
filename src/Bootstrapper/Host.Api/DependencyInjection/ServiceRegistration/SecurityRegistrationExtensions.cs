using Host.Api.DependencyInjection.ServiceRegistration.Web;

namespace Host.Api.DependencyInjection.ServiceRegistration;

public static class SecurityRegistrationsExtensions
{
    public static IServiceCollection AddSecurityRegistrations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddForwardedHeadersRegistration();
        services.AddCorsService(configuration);
        services.AddRateLimitingServices();
        services.AddAuthorizationPolicies();

        return services;
    }
}

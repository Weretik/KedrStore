namespace Host.Api.Security;

public static class SecurityRegistrationsExtensions
{
    public static IServiceCollection AddSecurityRegistrations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddForwardedHeadersRegistration();
        services.AddTrustedProxyRegistration(configuration);
        services.AddCorsService(configuration);
        services.AddRateLimitingServices();
        services.AddAuthorizationPolicies();

        return services;
    }
}

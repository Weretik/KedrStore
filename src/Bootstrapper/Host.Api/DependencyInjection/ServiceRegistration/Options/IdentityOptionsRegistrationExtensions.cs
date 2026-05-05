using Identity.Api.Options;
using Identity.Infrastructure.Options;

namespace Host.Api.DependencyInjection.ServiceRegistration.Options;

public static class IdentityOptionsRegistrationExtensions
{
    public static IServiceCollection AddIdentityConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AdminUserConfig>(
            configuration.GetSection("Identity:AdminUser"));
        services
            .AddOptions<SessionCookieOptions>()
            .Bind(configuration.GetSection(SessionCookieOptions.SectionName))
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.RefreshTokenCookieName) &&
                     !string.IsNullOrWhiteSpace(o.RefreshTokenCookiePath) &&
                     o.RefreshTokenCookieTtlDays > 0 &&
                     !string.IsNullOrWhiteSpace(o.CsrfCookieName) &&
                     !string.IsNullOrWhiteSpace(o.CsrfHeaderName) &&
                     !string.IsNullOrWhiteSpace(o.CsrfCookiePath) &&
                     o.CsrfCookieTtlDays > 0,
                "Identity:SessionCookies configuration is invalid.");

        services
            .AddOptions<IdentitySessionPerformanceOptions>()
            .Bind(configuration.GetSection(IdentitySessionPerformanceOptions.SectionName))
            .Validate(
                o => o.SlowStepThresholdMs > 0,
                "Identity:SessionPerformance configuration is invalid.");

        return services;
    }
}

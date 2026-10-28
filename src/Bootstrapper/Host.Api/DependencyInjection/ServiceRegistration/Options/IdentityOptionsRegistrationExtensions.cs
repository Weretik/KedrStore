using Identity.Api.Cookies;
using Identity.Api.Options;
using Identity.Infrastructure.Options;

namespace Host.Api.DependencyInjection.ServiceRegistration.Options;

public static class IdentityOptionsRegistrationExtensions
{
    public static IServiceCollection AddIdentityConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<SessionCookieOptions>()
            .Bind(configuration.GetSection(SessionCookieOptions.SectionName))
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.CsrfCookieName) &&
                     !string.IsNullOrWhiteSpace(o.CsrfHeaderName) &&
                     !string.IsNullOrWhiteSpace(o.CsrfCookiePath) &&
                     o.CsrfCookieTtlDays > 0,
                "Identity:SessionCookies configuration is invalid.");

        services.AddSingleton<SessionCookieWriter>();

        services
            .AddOptions<IdentityCookieSessionOptions>()
            .Bind(configuration.GetSection(IdentityCookieSessionOptions.SectionName))
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.CookieName) &&
                     o.LifetimeDays is > 0 and <= 90 &&
                     o.SecurityStampValidationMinutes is > 0 and <= 60 &&
                     (o.RequireSecure || !o.CookieName.StartsWith("__Host-", StringComparison.Ordinal)),
                "Identity:CookieSession configuration is invalid.")
            .ValidateOnStart();

        services
            .AddOptions<IdentitySessionPerformanceOptions>()
            .Bind(configuration.GetSection(IdentitySessionPerformanceOptions.SectionName))
            .Validate(
                o => o.SlowStepThresholdMs > 0,
                "Identity:SessionPerformance configuration is invalid.")
            .ValidateOnStart();

        services
            .AddOptions<IdentitySessionSecurityOptions>()
            .Bind(configuration.GetSection(IdentitySessionSecurityOptions.SectionName))
            .Validate(
                o => o.AccessTokenLifetimeMinutes is >= 5 and <= 60 &&
                     o.RefreshAbsoluteLifetimeDays is > 0 and <= 365 &&
                     o.RefreshIdleTimeoutDays is > 0 &&
                     o.RefreshIdleTimeoutDays <= o.RefreshAbsoluteLifetimeDays &&
                     o.RefreshReuseGraceSeconds is >= 0 and <= 300,
                "Identity:SessionSecurity configuration is invalid.")
            .ValidateOnStart();

        return services;
    }
}

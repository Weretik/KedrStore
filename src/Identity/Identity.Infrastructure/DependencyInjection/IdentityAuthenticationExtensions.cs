using Identity.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Identity.Infrastructure.DependencyInjection;

/// <summary>
/// Registers the Kedr scheme: bearer tokens for Mobile, the Identity cookie for browsers.
/// </summary>
public static class IdentityAuthenticationExtensions
{
    public const string KedrScheme = "Kedr";

    public static IServiceCollection AddIdentityAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var sessionSecurityOptions = configuration
            .GetSection(IdentitySessionSecurityOptions.SectionName)
            .Get<IdentitySessionSecurityOptions>() ?? new IdentitySessionSecurityOptions();
        var cookieSessionOptions = configuration
            .GetSection(IdentityCookieSessionOptions.SectionName)
            .Get<IdentityCookieSessionOptions>() ?? new IdentityCookieSessionOptions();

        var authentication = services
            .AddAuthentication(KedrScheme)
            .AddPolicyScheme(KedrScheme, KedrScheme, options =>
            {
                options.ForwardDefaultSelector = context => IsBearerRequest(context.Request)
                    ? IdentityConstants.BearerScheme
                    : IdentityConstants.ApplicationScheme;
            })
            .AddBearerToken(IdentityConstants.BearerScheme, options =>
            {
                options.BearerTokenExpiration = TimeSpan.FromMinutes(sessionSecurityOptions.AccessTokenLifetimeMinutes);
                options.RefreshTokenExpiration = TimeSpan.FromDays(sessionSecurityOptions.RefreshAbsoluteLifetimeDays);
            });

        // All Identity cookie schemes, because SignInManager.SignOutAsync signs out of each of them.
        authentication.AddIdentityCookies();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = cookieSessionOptions.CookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = cookieSessionOptions.RequireSecure
                ? CookieSecurePolicy.Always
                : CookieSecurePolicy.SameAsRequest;
            options.Cookie.IsEssential = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(cookieSessionOptions.LifetimeDays);
            options.SlidingExpiration = true;
            // An API answers with status codes; the frontends show the login or "no access" page.
            options.Events.OnRedirectToLogin = context => WriteStatusAsync(context, StatusCodes.Status401Unauthorized);
            options.Events.OnRedirectToAccessDenied = context => WriteStatusAsync(context, StatusCodes.Status403Forbidden);
        });

        services.Configure<SecurityStampValidatorOptions>(options =>
            options.ValidationInterval = TimeSpan.FromMinutes(cookieSessionOptions.SecurityStampValidationMinutes));

        return services;
    }

    /// <summary>A request with a bearer header is a Mobile request; any other one uses the browser cookie.</summary>
    public static bool IsBearerRequest(HttpRequest request) =>
        request.Headers.Authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);

    private static Task WriteStatusAsync(RedirectContext<CookieAuthenticationOptions> context, int statusCode)
    {
        context.Response.StatusCode = statusCode;
        return Task.CompletedTask;
    }
}

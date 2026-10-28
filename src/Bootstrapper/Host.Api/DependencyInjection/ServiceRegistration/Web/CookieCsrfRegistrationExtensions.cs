using Host.Api.Security.Csrf;

namespace Host.Api.DependencyInjection.ServiceRegistration.Web;

public static class CookieCsrfRegistrationExtensions
{
    public static IApplicationBuilder UseCookieCsrfProtection(this IApplicationBuilder app)
        => app.UseMiddleware<CookieCsrfMiddleware>();
}

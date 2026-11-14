using Host.Api.Security.Csrf;
namespace Host.Api.Security.Csrf;

public static class CookieCsrfRegistrationExtensions
{
    public static IApplicationBuilder UseCookieCsrfProtection(this IApplicationBuilder app)
        => app.UseMiddleware<CookieCsrfMiddleware>();
}

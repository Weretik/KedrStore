using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Identity.Api.Endpoints.Auth.GetSessionMe;

using Identity.Api.Endpoints.Auth.MobileLogout;
using Identity.Api.Endpoints.Auth.MobileRefresh;
using Identity.Api.Endpoints.Auth.CookieLogout;
using Identity.Api.Endpoints.Auth.MobileLogin;
using Identity.Api.Endpoints.Auth.CookieLogin;

namespace Identity.Api.Endpoints.Auth;

/// <summary>
/// Sign-in for every client: the browser session cookie with a readable CSRF cookie, mobile access and refresh
/// tokens in JSON, and the current user for both.
/// </summary>
public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var session = endpoints.MapGroup("api/auth/session")
            .WithTags("AuthSession")
            .RequireAuthorization();
        GetSessionMeEndpoint.Map(session);

        var cookie = endpoints.MapGroup("api/auth/cookie").WithTags("CookieAuth");
        CookieLoginEndpoint.Map(cookie);
        CookieLogoutEndpoint.Map(cookie);

        var mobile = endpoints.MapGroup("api/auth/mobile").WithTags("MobileAuth");
        MobileLoginEndpoint.Map(mobile);
        MobileRefreshEndpoint.Map(mobile);
        MobileLogoutEndpoint.Map(mobile);

        return endpoints;
    }
}

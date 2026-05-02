using Identity.Application.Security.Policies;
using Identity.Domain.Authorization;

namespace Host.Api.DependencyInjection.ServiceCollections.HostServices;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthentication(
        this IServiceCollection services,
        IConfiguration _)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(PolicyNames.RequireAdminRole, policy =>
                policy.RequireRole(RoleNames.Admin));

            options.AddPolicy(PolicyNames.RequireManagerRole, policy =>
                policy.RequireRole(RoleNames.Manager, RoleNames.Admin));

            options.AddPolicy(PolicyNames.CanManageUsers, policy =>
                policy.RequireRole(RoleNames.Admin));

            options.AddPolicy(PolicyNames.CanManageProducts, policy =>
                policy.RequireRole(RoleNames.Manager, RoleNames.Admin));

            options.AddPolicy(PolicyNames.CanManageOrders, policy =>
                policy.RequireRole(RoleNames.Manager, RoleNames.Admin));
        });

        return services;
    }
}

using Identity.Application.Security.Policies;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Host.Api.DependencyInjection.ServiceRegistration.Web;

public static class AuthorizationPoliciesRegistrationExtensions
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

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

            options.AddPolicy(PolicyNames.CatalogRead, policy =>
                policy.RequireRole(RoleNames.User, RoleNames.Manager, RoleNames.Admin));

            options.AddPolicy(PolicyNames.OrderCreate, policy =>
                policy.RequireRole(RoleNames.User, RoleNames.Manager, RoleNames.Admin));
        });

        return services;
    }
}

using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Authorization;
namespace Host.Api.Security;

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
                policy.RequireRole(RoleNames.Manager, RoleNames.Seo, RoleNames.Admin));

            options.AddPolicy(PolicyNames.CanManageUsers, policy =>
                policy.RequireRole(RoleNames.Admin));

            options.AddPolicy(PolicyNames.CanManageProducts, policy =>
                policy.RequireRole(RoleNames.Manager, RoleNames.Seo, RoleNames.Admin));

            options.AddPolicy(PolicyNames.CanManageOrders, policy =>
                policy.RequireRole(RoleNames.Manager, RoleNames.Seo, RoleNames.Admin));

            options.AddPolicy(PolicyNames.CanManageContent, policy =>
                policy.RequireRole(RoleNames.Manager, RoleNames.Seo, RoleNames.Admin));

            options.AddPolicy(PolicyNames.CanManageMedia, policy =>
                policy.RequireRole(RoleNames.Manager, RoleNames.Seo, RoleNames.Admin));

            options.AddPolicy(PolicyNames.CanManageSiteCatalog, policy =>
                policy.RequireRole(RoleNames.Seo, RoleNames.Admin));

            options.AddPolicy(PolicyNames.CatalogRead, policy =>
                policy.RequireRole(RoleNames.User, RoleNames.Manager, RoleNames.Seo, RoleNames.Admin));

            options.AddPolicy(PolicyNames.OrderCreate, policy =>
                policy.RequireRole(RoleNames.User, RoleNames.Manager, RoleNames.Seo, RoleNames.Admin));
        });

        return services;
    }
}

namespace Identity.Application.Security.Policies;

public static class PolicyNames
{
    public const string RequireAdminRole = "RequireAdminRole";
    public const string RequireManagerRole = "RequireManagerRole";
    public const string RequireTenantAccess = "RequireTenantAccess";
    public const string CanManageUsers = "CanManageUsers";
    public const string CanManageProducts = "CanManageProducts";
    public const string CanManageOrders = "CanManageOrders";
    public const string CatalogRead = "CatalogRead";
    public const string OrderCreate = "OrderCreate";
}

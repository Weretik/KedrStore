namespace Identity.Domain.Constants
{
    /// <summary>
    /// Константы для политик авторизации
    /// </summary>
    public static class AppPolicies
    {
        public const string RequireAdminRole = "RequireAdminRole";
        public const string RequireManagerRole = "RequireManagerRole";
        public const string RequireTenantAccess = "RequireTenantAccess";
        public const string CanManageUsers = "CanManageUsers";
        public const string CanManageProducts = "CanManageProducts";
        public const string CanManageOrders = "CanManageOrders";
    }
}

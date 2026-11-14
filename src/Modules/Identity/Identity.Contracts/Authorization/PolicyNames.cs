namespace Identity.Contracts.Authorization;

public static class PolicyNames
{
    public const string RequireAdminRole = "RequireAdminRole";
    public const string RequireManagerRole = "RequireManagerRole";
    public const string RequireTenantAccess = "RequireTenantAccess";
    public const string CanManageUsers = "CanManageUsers";
    public const string CanManageProducts = "CanManageProducts";
    public const string CanManageOrders = "CanManageOrders";
    public const string CanManageContent = "CanManageContent";
    public const string CanManageMedia = "CanManageMedia";

    /// <summary>Site-facing catalog structure: addresses, category texts, visibility, attributes, deleted rows.</summary>
    public const string CanManageSiteCatalog = "CanManageSiteCatalog";
    public const string CatalogRead = "CatalogRead";
    public const string OrderCreate = "OrderCreate";
}

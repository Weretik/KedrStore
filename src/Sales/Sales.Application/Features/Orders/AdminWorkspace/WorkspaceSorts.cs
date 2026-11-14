namespace Sales.Application.Features.Orders.AdminWorkspace;

/// <summary>The sort keys the workspace lists accept; a leading <c>-</c> sorts in descending order.</summary>
public static class WorkspaceSorts
{
    public static readonly IReadOnlySet<string> Orders = new HashSet<string>(StringComparer.Ordinal)
    {
        "createdAt", "-createdAt", "orderNumber", "-orderNumber", "customerName", "-customerName",
        "totalAmount", "-totalAmount",
    };

    public static readonly IReadOnlySet<string> Drafts = new HashSet<string>(StringComparer.Ordinal)
    {
        "updatedAt", "-updatedAt", "title", "-title", "customerName", "-customerName",
    };
}

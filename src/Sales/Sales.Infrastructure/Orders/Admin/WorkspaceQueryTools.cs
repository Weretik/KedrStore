using Sales.Application.Contracts.Orders;

namespace Sales.Infrastructure.Orders;

internal static class WorkspaceQueryTools
{
    internal static WorkspacePagedInfo PageInfo(int page, int pageSize, long count)
        => new(page, pageSize, (count + pageSize - 1) / pageSize, count);

    internal static void ValidatePage(string? search, string? customer, int page, int size)
    {
        if (page < 1 || size is < 1 or > 100 || search?.Length > 100 || customer?.Length > 64)
            throw new WorkspaceException(400, "invalid_query", "Page, search or customer filter is invalid.");
    }

    internal static IEnumerable<string> Terms(string? search)
        => (search ?? "").Split((char[]?)null, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    internal static string EscapeLike(string term)
        => term.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}

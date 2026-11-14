using Sales.Application.Abstractions.Orders;

namespace Sales.Infrastructure.Orders;

internal static class WorkspaceQueryTools
{
    internal static WorkspacePagedInfo PageInfo(int page, int pageSize, long count)
        => new(page, pageSize, (count + pageSize - 1) / pageSize, count);

    internal static IEnumerable<string> Terms(string? search)
        => (search ?? "").Split((char[]?)null, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    internal static string EscapeLike(string term)
        => term.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}

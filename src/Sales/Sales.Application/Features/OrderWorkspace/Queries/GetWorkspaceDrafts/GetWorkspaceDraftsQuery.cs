using Sales.Application.Abstractions.Orders;

namespace Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceDrafts;

/// <param name="CreatedFrom">Inclusive Kyiv calendar date, <c>yyyy-MM-dd</c>.</param>
/// <param name="CreatedTo">Inclusive Kyiv calendar date, <c>yyyy-MM-dd</c>.</param>
public sealed record GetWorkspaceDraftsQuery(string? SearchTerm, string? CounterpartyId,
    string? CreatedFrom, string? CreatedTo,
    string Sort, int Page, int PageSize) : IQuery<Result<WorkspacePage<WorkspaceDraftListItem>>>;

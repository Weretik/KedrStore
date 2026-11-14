using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Drafts.GetList;

/// <param name="CreatedFrom">Inclusive Kyiv calendar date, <c>yyyy-MM-dd</c>.</param>
/// <param name="CreatedTo">Inclusive Kyiv calendar date, <c>yyyy-MM-dd</c>.</param>
public sealed record GetWorkspaceDraftsQuery(string? SearchTerm, string? CounterpartyId,
    string? CreatedFrom, string? CreatedTo,
    string Sort, int Page, int PageSize) : IQuery<Result<WorkspacePage<WorkspaceDraftListItem>>>;

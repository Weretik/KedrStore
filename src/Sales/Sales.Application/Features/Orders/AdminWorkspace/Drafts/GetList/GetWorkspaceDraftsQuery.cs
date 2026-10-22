using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Drafts.GetList;

public sealed record GetWorkspaceDraftsQuery(string? SearchTerm, string? CounterpartyId,
    DateTimeOffset? CreatedFromUtc, DateTimeOffset? CreatedToUtcExclusive,
    string Sort, int Page, int PageSize) : IQuery<Result<WorkspacePage<WorkspaceDraftListItem>>>;

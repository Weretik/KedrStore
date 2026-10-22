using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Orders.GetList;

public sealed record GetWorkspaceOrdersQuery(string? SearchTerm, string? CounterpartyId,
    DateTimeOffset? CreatedFromUtc, DateTimeOffset? CreatedToUtcExclusive,
    string Sort, int Page, int PageSize) : IQuery<Result<WorkspacePage<WorkspaceOrderListItem>>>;

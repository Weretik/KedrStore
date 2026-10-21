using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Drafts.GetById;

public sealed record GetWorkspaceDraftQuery(long DraftId) : IQuery<Result<WorkspaceDraftDetail>>;

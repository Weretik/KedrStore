using Sales.Application.Abstractions.Orders;

namespace Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceDraft;

public sealed record GetWorkspaceDraftQuery(long DraftId) : IQuery<Result<WorkspaceDraftDetail>>;

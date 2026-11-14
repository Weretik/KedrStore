using Sales.Application.Abstractions.Orders;
using Sales.Application.Abstractions.Workspace.Drafts;

namespace Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceDraft;

public sealed record GetWorkspaceDraftQuery(long DraftId) : IQuery<Result<WorkspaceDraftDetail>>;

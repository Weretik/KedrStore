using Sales.Application.Abstractions.Orders;
using Sales.Application.Abstractions.Workspace.Drafts;

namespace Sales.Application.Features.OrderWorkspace.Commands.UpdateWorkspaceDraft;

public sealed record UpdateWorkspaceDraftCommand(long DraftId, string Version, WorkspaceDraftInput Draft) : ICommand<Result<WorkspaceDraftDetail>>;

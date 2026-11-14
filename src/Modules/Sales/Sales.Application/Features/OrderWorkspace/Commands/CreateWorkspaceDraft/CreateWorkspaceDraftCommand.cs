using Sales.Application.Abstractions.Orders;
using Sales.Application.Abstractions.Workspace.Drafts;

namespace Sales.Application.Features.OrderWorkspace.Commands.CreateWorkspaceDraft;

public sealed record CreateWorkspaceDraftCommand(WorkspaceDraftInput Draft, Guid IdempotencyKey) : ICommand<Result<(WorkspaceDraftDetail Draft, bool Replay)>>;

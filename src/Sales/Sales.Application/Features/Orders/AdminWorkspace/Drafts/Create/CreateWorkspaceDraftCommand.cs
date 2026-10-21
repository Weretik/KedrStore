using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Drafts.Create;

public sealed record CreateWorkspaceDraftCommand(WorkspaceDraftInput Draft, Guid IdempotencyKey) : ICommand<Result<(WorkspaceDraftDetail Draft, bool Replay)>>;

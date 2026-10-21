using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Drafts.Update;

public sealed record UpdateWorkspaceDraftCommand(long DraftId, string Version, WorkspaceDraftInput Draft) : ICommand<Result<WorkspaceDraftDetail>>;

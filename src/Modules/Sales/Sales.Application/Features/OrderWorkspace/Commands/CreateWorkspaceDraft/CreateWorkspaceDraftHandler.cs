using Sales.Application.Abstractions.Orders;
using Sales.Application.Abstractions.Workspace.Drafts;

namespace Sales.Application.Features.OrderWorkspace.Commands.CreateWorkspaceDraft;

public sealed class CreateWorkspaceDraftHandler(IWorkspaceDraftWriter port)
    : ICommandHandler<CreateWorkspaceDraftCommand, Result<(WorkspaceDraftDetail Draft, bool Replay)>>
{
    public async ValueTask<Result<(WorkspaceDraftDetail Draft, bool Replay)>> Handle(CreateWorkspaceDraftCommand request, CancellationToken cancellationToken)
        => (await port.CreateDraftAsync(request.Draft, request.IdempotencyKey, cancellationToken)).AsResult();
}

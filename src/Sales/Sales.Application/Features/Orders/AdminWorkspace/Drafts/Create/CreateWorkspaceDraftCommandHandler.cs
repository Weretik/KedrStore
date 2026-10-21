using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Drafts.Create;

public sealed class CreateWorkspaceDraftCommandHandler(IWorkspaceDraftWriter port)
    : ICommandHandler<CreateWorkspaceDraftCommand, Result<(WorkspaceDraftDetail Draft, bool Replay)>>
{
    public async ValueTask<Result<(WorkspaceDraftDetail Draft, bool Replay)>> Handle(CreateWorkspaceDraftCommand request, CancellationToken cancellationToken)
    {
        return Result.Success(await port.CreateDraftAsync(request.Draft, request.IdempotencyKey, cancellationToken));
    }
}

using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Drafts.Delete;

public sealed class DeleteWorkspaceDraftCommandHandler(IWorkspaceDraftWriter port)
    : ICommandHandler<DeleteWorkspaceDraftCommand, Result<bool>>
{
    public async ValueTask<Result<bool>> Handle(DeleteWorkspaceDraftCommand request, CancellationToken cancellationToken)
    {
        return Result.Success(await port.DeleteDraftAsync(request.DraftId, request.Version, cancellationToken));
    }
}

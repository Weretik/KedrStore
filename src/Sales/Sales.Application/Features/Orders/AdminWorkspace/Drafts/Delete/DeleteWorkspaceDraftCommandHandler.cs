using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Drafts.Delete;

public sealed class DeleteWorkspaceDraftCommandHandler(IWorkspaceDraftWriter port)
    : ICommandHandler<DeleteWorkspaceDraftCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteWorkspaceDraftCommand request, CancellationToken cancellationToken)
        => (await port.DeleteDraftAsync(request.DraftId, request.Version, cancellationToken)).ToResult();
}

using Sales.Application.Abstractions.Orders;
using Sales.Application.Abstractions.Workspace.Drafts;

namespace Sales.Application.Features.OrderWorkspace.Commands.DeleteWorkspaceDraft;

public sealed class DeleteWorkspaceDraftHandler(IWorkspaceDraftWriter port)
    : ICommandHandler<DeleteWorkspaceDraftCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteWorkspaceDraftCommand request, CancellationToken cancellationToken)
        => (await port.DeleteDraftAsync(request.DraftId, request.Version, cancellationToken)).ToResult();
}

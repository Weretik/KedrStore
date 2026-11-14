using Sales.Application.Abstractions.Orders;
using Sales.Application.Abstractions.Workspace.Drafts;

namespace Sales.Application.Features.OrderWorkspace.Commands.UpdateWorkspaceDraft;

public sealed class UpdateWorkspaceDraftHandler(IWorkspaceDraftWriter port)
    : ICommandHandler<UpdateWorkspaceDraftCommand, Result<WorkspaceDraftDetail>>
{
    public async ValueTask<Result<WorkspaceDraftDetail>> Handle(UpdateWorkspaceDraftCommand request, CancellationToken cancellationToken)
        => (await port.UpdateDraftAsync(request.DraftId, request.Version, request.Draft, cancellationToken)).AsResult();
}

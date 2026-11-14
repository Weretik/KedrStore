using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Drafts.Update;

public sealed class UpdateWorkspaceDraftCommandHandler(IWorkspaceDraftWriter port)
    : ICommandHandler<UpdateWorkspaceDraftCommand, Result<WorkspaceDraftDetail>>
{
    public async ValueTask<Result<WorkspaceDraftDetail>> Handle(UpdateWorkspaceDraftCommand request, CancellationToken cancellationToken)
        => (await port.UpdateDraftAsync(request.DraftId, request.Version, request.Draft, cancellationToken)).AsResult();
}

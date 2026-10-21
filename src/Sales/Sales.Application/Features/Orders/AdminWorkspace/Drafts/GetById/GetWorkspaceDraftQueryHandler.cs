using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Drafts.GetById;

public sealed class GetWorkspaceDraftQueryHandler(IWorkspaceDraftReader port)
    : IQueryHandler<GetWorkspaceDraftQuery, Result<WorkspaceDraftDetail>>
{
    public async ValueTask<Result<WorkspaceDraftDetail>> Handle(GetWorkspaceDraftQuery request, CancellationToken cancellationToken)
    {
        var value = await port.GetDraftAsync(request.DraftId, cancellationToken);
        return value is null ? Result.NotFound() : Result.Success(value);
    }
}

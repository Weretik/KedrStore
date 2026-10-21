using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Orders.Create;

public sealed class CreateWorkspaceOrderCommandHandler(IWorkspaceOrderCreator port)
    : ICommandHandler<CreateWorkspaceOrderCommand, Result<(WorkspaceCreatedOrder Order, bool Replay, WorkspaceOrderConflict? Conflict)>>
{
    public async ValueTask<Result<(WorkspaceCreatedOrder Order, bool Replay, WorkspaceOrderConflict? Conflict)>> Handle(CreateWorkspaceOrderCommand request, CancellationToken cancellationToken)
    {
        return Result.Success(await port.CreateOrderAsync(request.Order, request.IdempotencyKey, cancellationToken));
    }
}

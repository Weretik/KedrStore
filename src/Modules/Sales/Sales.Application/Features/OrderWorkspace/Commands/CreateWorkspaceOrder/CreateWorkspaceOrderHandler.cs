using Sales.Application.Abstractions.Orders;

namespace Sales.Application.Features.OrderWorkspace.Commands.CreateWorkspaceOrder;

public sealed class CreateWorkspaceOrderHandler(IWorkspaceOrderCreator port)
    : ICommandHandler<CreateWorkspaceOrderCommand, Result<(WorkspaceCreatedOrder Order, bool Replay, WorkspaceOrderConflict? Conflict)>>
{
    public async ValueTask<Result<(WorkspaceCreatedOrder Order, bool Replay, WorkspaceOrderConflict? Conflict)>> Handle(CreateWorkspaceOrderCommand request, CancellationToken cancellationToken)
        => (await port.CreateOrderAsync(request.Order, request.IdempotencyKey, cancellationToken)).AsResult();
}

using Sales.Application.Features.Orders.Commands.RetryOrderSync;

namespace Sales.Application.Features.Orders.Commands.RetryOrderSync;

public sealed record RetryOrderSyncCommand(
    long OrderId,
    string Reason,
    string RequestedBy) : ICommand<Result<RetryOrderSyncResult>>;

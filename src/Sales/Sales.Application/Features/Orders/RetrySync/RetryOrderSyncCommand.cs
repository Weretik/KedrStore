using Sales.Application.Features.Orders.RetrySync.DTOs;

namespace Sales.Application.Features.Orders.RetrySync;

public sealed record RetryOrderSyncCommand(
    long OrderId,
    string Reason,
    string RequestedBy) : ICommand<Result<RetryOrderSyncResult>>;

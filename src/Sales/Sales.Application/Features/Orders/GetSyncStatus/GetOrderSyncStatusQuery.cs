using Sales.Application.Features.Orders.GetSyncStatus.DTOs;

namespace Sales.Application.Features.Orders.GetSyncStatus;

public sealed record GetOrderSyncStatusQuery(long OrderId) : IQuery<Result<GetOrderSyncStatusResult>>;

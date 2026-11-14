using Sales.Application.Features.Orders.Queries.GetOrderSyncStatus;

namespace Sales.Application.Features.Orders.Queries.GetOrderSyncStatus;

public sealed record GetOrderSyncStatusQuery(long OrderId) : IQuery<Result<GetOrderSyncStatusResult>>;

namespace Sales.Application.Features.QuickOrders.Queries.GetQuickOrderById;

public sealed record GetQuickOrderByIdQuery(Guid OrderId) : IQuery<Result<QuickOrderDetailDto>>;

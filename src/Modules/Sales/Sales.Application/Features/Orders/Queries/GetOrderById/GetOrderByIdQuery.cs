using Sales.Application.Features.Orders.Queries.GetOrderById;

namespace Sales.Application.Features.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(long OrderId) : IQuery<Result<OrderDetailDto>>;

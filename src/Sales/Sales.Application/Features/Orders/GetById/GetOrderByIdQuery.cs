using Sales.Application.Features.Orders.GetById.DTOs;

namespace Sales.Application.Features.Orders.GetById;

public sealed record GetOrderByIdQuery(long OrderId) : IQuery<Result<OrderDetailDto>>;

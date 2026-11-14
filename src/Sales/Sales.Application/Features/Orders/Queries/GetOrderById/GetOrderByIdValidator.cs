namespace Sales.Application.Features.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdValidator : AbstractValidator<GetOrderByIdQuery>
{
    public GetOrderByIdValidator()
    {
        RuleFor(query => query.OrderId).GreaterThan(0);
    }
}

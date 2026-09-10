namespace Sales.Application.Features.Orders.GetById.Validators;

public sealed class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
{
    public GetOrderByIdQueryValidator()
    {
        RuleFor(query => query.OrderId).GreaterThan(0);
    }
}

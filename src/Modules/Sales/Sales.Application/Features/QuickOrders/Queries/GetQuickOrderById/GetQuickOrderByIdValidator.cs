namespace Sales.Application.Features.QuickOrders.Queries.GetQuickOrderById;

public sealed class GetQuickOrderByIdValidator : AbstractValidator<GetQuickOrderByIdQuery>
{
    public GetQuickOrderByIdValidator()
    {
        RuleFor(query => query.OrderId)
            .NotEqual(Guid.Empty).WithMessage("Некоректний номер замовлення.");
    }
}

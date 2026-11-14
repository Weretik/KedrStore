namespace Sales.Application.Features.Orders.Queries.GetOrderList;

public sealed class GetOrderListValidator : AbstractValidator<GetOrderListQuery>
{
    public GetOrderListValidator()
    {
        RuleFor(query => query.Request).NotNull();
        When(query => query.Request is not null, () =>
        {
            RuleFor(query => query.Request.CounterpartyId)
                .Must(value => value is null || !string.IsNullOrWhiteSpace(value))
                .WithMessage("Вкажіть контрагента.")
                .MaximumLength(64);
            RuleFor(query => query.Request.Page).GreaterThanOrEqualTo(1);
            RuleFor(query => query.Request.PageSize).InclusiveBetween(1, 100);
        });
    }
}

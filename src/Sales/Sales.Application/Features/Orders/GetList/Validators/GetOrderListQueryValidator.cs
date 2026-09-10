namespace Sales.Application.Features.Orders.GetList.Validators;

public sealed class GetOrderListQueryValidator : AbstractValidator<GetOrderListQuery>
{
    public GetOrderListQueryValidator()
    {
        RuleFor(query => query.Request).NotNull();
        When(query => query.Request is not null, () =>
        {
            RuleFor(query => query.Request.CounterpartyId)
                .Must(value => value is null || !string.IsNullOrWhiteSpace(value))
                .WithMessage("Counterparty ID must not be blank.")
                .MaximumLength(64);
            RuleFor(query => query.Request.Page).GreaterThanOrEqualTo(1);
            RuleFor(query => query.Request.PageSize).InclusiveBetween(1, 100);
        });
    }
}

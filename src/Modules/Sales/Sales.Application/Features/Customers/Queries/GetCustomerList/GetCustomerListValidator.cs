namespace Sales.Application.Features.Customers.Queries.GetCustomerList;

public sealed class GetCustomerListValidator : AbstractValidator<GetCustomerListQuery>
{
    public GetCustomerListValidator()
    {
        RuleFor(query => query.Request).NotNull();
        When(query => query.Request is not null, () =>
        {
            RuleFor(query => query.Request.Page).GreaterThanOrEqualTo(1);
            RuleFor(query => query.Request.PageSize).InclusiveBetween(1, 100);
            RuleFor(query => query.Request.SearchTerm).MaximumLength(100);
            RuleFor(query => query.Request.CounterpartyId).MaximumLength(100);
            RuleFor(query => query.Request.Name).MaximumLength(100);
            RuleFor(query => query.Request.Phone).MaximumLength(100);
            RuleFor(query => query.Request.Sort)
                .NotEmpty()
                .Must(sort => sort is "name" or "-name" or "counterpartyId" or "-counterpartyId" or "phone" or "-phone");
        });
    }
}

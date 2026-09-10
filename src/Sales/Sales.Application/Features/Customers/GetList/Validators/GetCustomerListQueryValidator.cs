namespace Sales.Application.Features.Customers.GetList.Validators;

public sealed class GetCustomerListQueryValidator : AbstractValidator<GetCustomerListQuery>
{
    public GetCustomerListQueryValidator()
    {
        RuleFor(query => query.Request).NotNull();
        When(query => query.Request is not null, () =>
        {
            RuleFor(query => query.Request.Page).GreaterThanOrEqualTo(1);
            RuleFor(query => query.Request.PageSize).InclusiveBetween(1, 100);
        });
    }
}

namespace Sales.Application.Features.Customers.Queries.GetCustomerById;

public sealed class GetCustomerByIdValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdValidator()
    {
        RuleFor(query => query.CounterpartyId)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Вкажіть контрагента.")
            .MaximumLength(64);
    }
}

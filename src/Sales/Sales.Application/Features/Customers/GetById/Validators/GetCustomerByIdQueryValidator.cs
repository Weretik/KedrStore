namespace Sales.Application.Features.Customers.GetById.Validators;

public sealed class GetCustomerByIdQueryValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdQueryValidator()
    {
        RuleFor(query => query.CounterpartyId)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Вкажіть контрагента.")
            .MaximumLength(64);
    }
}

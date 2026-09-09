namespace Sales.Application.Features.Orders.Create.Validators;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(command => command.IdempotencyKey).NotEmpty();

        RuleFor(command => command.Request).NotNull();

        When(command => command.Request is not null, () =>
        {
            RuleFor(command => command.Request.CounterpartyId)
                .NotEmpty()
                .MaximumLength(64);

            RuleFor(command => command.Request.Comment)
                .MaximumLength(1_000);

            RuleFor(command => command.Request.Lines)
                .NotEmpty();

            RuleForEach(command => command.Request.Lines).ChildRules(line =>
            {
                line.RuleFor(request => request.ProductId)
                    .NotEmpty()
                    .MaximumLength(64);
                line.RuleFor(request => request.Quantity).GreaterThan(0);
                line.RuleFor(request => request.Amount).GreaterThanOrEqualTo(0m);
            });
        });
    }
}

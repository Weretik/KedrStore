using Catalog.Application.Features.Orders.Create.DTOs;

namespace Catalog.Application.Features.Orders.Create.Validators;

public sealed class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(15);

        RuleFor(x => x.Lines).NotNull().NotEmpty();
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(x => x.ProductId).NotEmpty().MaximumLength(64);
            line.RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            line.RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(1000);
        });
    }
}

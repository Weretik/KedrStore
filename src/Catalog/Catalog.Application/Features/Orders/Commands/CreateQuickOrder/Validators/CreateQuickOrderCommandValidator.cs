using Catalog.Application.Features.Orders.Commands.CreateQuickOrder.Contracts;

namespace Catalog.Application.Features.Orders.Commands.CreateQuickOrder.Validators;

public sealed class CreateQuickOrderCommandValidator : AbstractValidator<CreateQuickOrderCommand>
{
    public CreateQuickOrderCommandValidator(IValidator<IQuickOrder> requestValidator)
    {
        RuleFor(сommand => сommand.Request).SetValidator(requestValidator);
    }
}

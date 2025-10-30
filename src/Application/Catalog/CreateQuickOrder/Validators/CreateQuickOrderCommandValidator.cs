using Application.Catalog.CreateQuickOrder.Contracts;

namespace Application.Catalog.CreateQuickOrder.Validators;

public sealed class CreateQuickOrderCommandValidator : AbstractValidator<CreateQuickOrderCommand>
{
    public CreateQuickOrderCommandValidator(IValidator<IQuickOrder> requestValidator)
    {
        RuleFor(сommand => сommand.Request).SetValidator(requestValidator);
    }
}

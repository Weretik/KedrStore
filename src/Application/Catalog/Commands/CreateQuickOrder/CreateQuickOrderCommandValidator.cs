namespace Application.Catalog.Commands.CreateQuickOrder;

public sealed class CreateQuickOrderCommandValidator : AbstractValidator<CreateQuickOrderCommand>
{
    public CreateQuickOrderCommandValidator(IValidator<IQuickOrder> requestValidator)
    {
        RuleFor(сommand => сommand.Request).SetValidator(requestValidator);
    }
}

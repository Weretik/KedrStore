using Sales.Domain.Orders.Entities;

namespace Sales.Application.Features.Orders.AdminWorkspace.Orders.Create.Validators;

public sealed class CreateWorkspaceOrderCommandValidator : AbstractValidator<CreateWorkspaceOrderCommand>
{
    private const string Message = "Оновіть сторінку й надішліть замовлення ще раз.";

    public CreateWorkspaceOrderCommandValidator()
    {
        RuleFor(command => command.IdempotencyKey).NotEmpty().WithErrorCode("invalid_request").WithMessage(Message);
        RuleFor(command => command.Order).NotNull().WithErrorCode("invalid_request").WithMessage(Message);
        When(command => command.Order is not null, () =>
        {
            RuleFor(command => command.Order.QuoteFingerprint).NotEmpty().WithErrorCode("invalid_request")
                .WithMessage(Message);
            RuleFor(command => command.Order.Comment).MaximumLength(Order.CommentMaxLength)
                .WithErrorCode("invalid_request")
                .WithMessage($"Коментар — не більше {Order.CommentMaxLength} символів.");
            this.AddLineRules(command => command.Order.CounterpartyId, command => command.Order.Lines);
        });
    }
}

using Sales.Domain.Orders;
using Sales.Application.Features.OrderWorkspace.Shared;

namespace Sales.Application.Features.OrderWorkspace.Commands.CreateWorkspaceOrder;

public sealed class CreateWorkspaceOrderValidator : AbstractValidator<CreateWorkspaceOrderCommand>
{
    private const string Message = "Оновіть сторінку й надішліть замовлення ще раз.";

    public CreateWorkspaceOrderValidator()
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

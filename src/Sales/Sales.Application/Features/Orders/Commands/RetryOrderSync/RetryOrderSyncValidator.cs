namespace Sales.Application.Features.Orders.Commands.RetryOrderSync;

public sealed class RetryOrderSyncValidator : AbstractValidator<RetryOrderSyncCommand>
{
    public RetryOrderSyncValidator()
    {
        RuleFor(command => command.OrderId).GreaterThan(0);
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(500);
        RuleFor(command => command.RequestedBy).NotEmpty().MaximumLength(255);
    }
}

namespace Sales.Application.Features.Orders.RetrySync.Validators;

public sealed class RetryOrderSyncCommandValidator : AbstractValidator<RetryOrderSyncCommand>
{
    public RetryOrderSyncCommandValidator()
    {
        RuleFor(command => command.OrderId).GreaterThan(0);
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(500);
        RuleFor(command => command.RequestedBy).NotEmpty().MaximumLength(255);
    }
}

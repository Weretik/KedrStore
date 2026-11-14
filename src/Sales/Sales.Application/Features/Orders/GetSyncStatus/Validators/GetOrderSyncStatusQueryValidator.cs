namespace Sales.Application.Features.Orders.GetSyncStatus.Validators;

public sealed class GetOrderSyncStatusQueryValidator : AbstractValidator<GetOrderSyncStatusQuery>
{
    public GetOrderSyncStatusQueryValidator()
    {
        RuleFor(query => query.OrderId).GreaterThan(0);
    }
}

namespace Sales.Application.Features.Orders.Queries.GetOrderSyncStatus;

public sealed class GetOrderSyncStatusValidator : AbstractValidator<GetOrderSyncStatusQuery>
{
    public GetOrderSyncStatusValidator()
    {
        RuleFor(query => query.OrderId).GreaterThan(0);
    }
}

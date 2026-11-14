namespace Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceOrder;

public sealed class GetWorkspaceOrderValidator : AbstractValidator<GetWorkspaceOrderQuery>
{
    public GetWorkspaceOrderValidator()
    {
        RuleFor(query => query.OrderId).GreaterThan(0);
    }
}

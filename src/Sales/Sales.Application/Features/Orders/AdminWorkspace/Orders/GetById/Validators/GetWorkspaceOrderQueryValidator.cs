namespace Sales.Application.Features.Orders.AdminWorkspace.Orders.GetById.Validators;

public sealed class GetWorkspaceOrderQueryValidator : AbstractValidator<GetWorkspaceOrderQuery>
{
    public GetWorkspaceOrderQueryValidator()
    {
        RuleFor(query => query.OrderId).GreaterThan(0);
    }
}

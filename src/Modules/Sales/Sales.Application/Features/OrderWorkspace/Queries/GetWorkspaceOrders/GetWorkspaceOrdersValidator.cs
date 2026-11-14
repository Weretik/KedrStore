using Sales.Application.Features.OrderWorkspace.Shared;

namespace Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceOrders;

public sealed class GetWorkspaceOrdersValidator : AbstractValidator<GetWorkspaceOrdersQuery>
{
    public GetWorkspaceOrdersValidator()
    {
        this.AddListRules(query => query.SearchTerm, query => query.CounterpartyId, query => query.Page,
            query => query.PageSize);
        this.AddDateRangeRule(query => query.CreatedFrom, query => query.CreatedTo);
        this.AddSortRule(query => query.Sort, WorkspaceSorts.Orders);
    }
}

namespace Sales.Application.Features.Orders.AdminWorkspace.Orders.GetList.Validators;

public sealed class GetWorkspaceOrdersQueryValidator : AbstractValidator<GetWorkspaceOrdersQuery>
{
    public GetWorkspaceOrdersQueryValidator()
    {
        this.AddListRules(query => query.SearchTerm, query => query.CounterpartyId, query => query.Page,
            query => query.PageSize);
        this.AddDateRangeRule(query => query.CreatedFrom, query => query.CreatedTo);
        this.AddSortRule(query => query.Sort, WorkspaceSorts.Orders);
    }
}

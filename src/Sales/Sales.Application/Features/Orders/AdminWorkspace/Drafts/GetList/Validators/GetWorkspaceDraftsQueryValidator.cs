namespace Sales.Application.Features.Orders.AdminWorkspace.Drafts.GetList.Validators;

public sealed class GetWorkspaceDraftsQueryValidator : AbstractValidator<GetWorkspaceDraftsQuery>
{
    public GetWorkspaceDraftsQueryValidator()
    {
        this.AddListRules(query => query.SearchTerm, query => query.CounterpartyId, query => query.Page,
            query => query.PageSize);
        this.AddDateRangeRule(query => query.CreatedFrom, query => query.CreatedTo);
        this.AddSortRule(query => query.Sort, WorkspaceSorts.Drafts);
    }
}

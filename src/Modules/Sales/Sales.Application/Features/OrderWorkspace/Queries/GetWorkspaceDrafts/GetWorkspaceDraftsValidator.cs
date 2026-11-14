using Sales.Application.Features.OrderWorkspace.Shared;

namespace Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceDrafts;

public sealed class GetWorkspaceDraftsValidator : AbstractValidator<GetWorkspaceDraftsQuery>
{
    public GetWorkspaceDraftsValidator()
    {
        this.AddListRules(query => query.SearchTerm, query => query.CounterpartyId, query => query.Page,
            query => query.PageSize);
        this.AddDateRangeRule(query => query.CreatedFrom, query => query.CreatedTo);
        this.AddSortRule(query => query.Sort, WorkspaceSorts.Drafts);
    }
}

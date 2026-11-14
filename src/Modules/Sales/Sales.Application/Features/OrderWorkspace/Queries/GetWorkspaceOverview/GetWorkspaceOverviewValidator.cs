namespace Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceOverview;

public sealed class GetWorkspaceOverviewValidator : AbstractValidator<GetWorkspaceOverviewQuery>
{
    public GetWorkspaceOverviewValidator()
    {
        RuleFor(query => query.TodayFromUtc).NotEqual(default(DateTimeOffset));
    }
}

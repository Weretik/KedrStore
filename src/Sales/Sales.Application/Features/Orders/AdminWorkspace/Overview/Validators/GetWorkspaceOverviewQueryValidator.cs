namespace Sales.Application.Features.Orders.AdminWorkspace.Overview.Validators;

public sealed class GetWorkspaceOverviewQueryValidator : AbstractValidator<GetWorkspaceOverviewQuery>
{
    public GetWorkspaceOverviewQueryValidator()
    {
        RuleFor(query => query.TodayFromUtc).NotEqual(default(DateTimeOffset));
    }
}

namespace Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceDraft;

public sealed class GetWorkspaceDraftValidator : AbstractValidator<GetWorkspaceDraftQuery>
{
    public GetWorkspaceDraftValidator()
    {
        RuleFor(query => query.DraftId).GreaterThan(0);
    }
}

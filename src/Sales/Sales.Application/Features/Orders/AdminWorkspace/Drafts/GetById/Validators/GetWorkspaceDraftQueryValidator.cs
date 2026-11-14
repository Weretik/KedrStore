namespace Sales.Application.Features.Orders.AdminWorkspace.Drafts.GetById.Validators;

public sealed class GetWorkspaceDraftQueryValidator : AbstractValidator<GetWorkspaceDraftQuery>
{
    public GetWorkspaceDraftQueryValidator()
    {
        RuleFor(query => query.DraftId).GreaterThan(0);
    }
}

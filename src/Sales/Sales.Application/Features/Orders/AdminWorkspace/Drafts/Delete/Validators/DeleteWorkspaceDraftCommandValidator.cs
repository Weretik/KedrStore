namespace Sales.Application.Features.Orders.AdminWorkspace.Drafts.Delete.Validators;

public sealed class DeleteWorkspaceDraftCommandValidator : AbstractValidator<DeleteWorkspaceDraftCommand>
{
    public DeleteWorkspaceDraftCommandValidator()
    {
        RuleFor(command => command.DraftId).GreaterThan(0);
    }
}

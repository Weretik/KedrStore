namespace Sales.Application.Features.OrderWorkspace.Commands.DeleteWorkspaceDraft;

public sealed class DeleteWorkspaceDraftValidator : AbstractValidator<DeleteWorkspaceDraftCommand>
{
    public DeleteWorkspaceDraftValidator()
    {
        RuleFor(command => command.DraftId).GreaterThan(0);
    }
}

namespace Sales.Application.Features.OrderWorkspace.Commands.DeleteWorkspaceDraft;

public sealed class DeleteWorkspaceDraftValidator : AbstractValidator<DeleteWorkspaceDraftCommand>
{
    public DeleteWorkspaceDraftValidator()
    {
        RuleFor(command => command.DraftId).GreaterThan(0);
        // The If-Match header: without it the draft cannot be checked for a newer save.
        RuleFor(command => command.Version).NotEmpty().WithErrorCode("invalid_request")
            .WithMessage("Немає версії чернетки. Оновіть сторінку.");
    }
}

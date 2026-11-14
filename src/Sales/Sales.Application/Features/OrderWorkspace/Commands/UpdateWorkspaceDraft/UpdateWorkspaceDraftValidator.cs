using Sales.Application.Features.OrderWorkspace.Shared;

namespace Sales.Application.Features.OrderWorkspace.Commands.UpdateWorkspaceDraft;

public sealed class UpdateWorkspaceDraftValidator : AbstractValidator<UpdateWorkspaceDraftCommand>
{
    public UpdateWorkspaceDraftValidator()
    {
        RuleFor(command => command.DraftId).GreaterThan(0);
        // The If-Match header: without it the draft cannot be checked for a newer save.
        RuleFor(command => command.Version).NotEmpty().WithErrorCode("invalid_request")
            .WithMessage("Немає версії чернетки. Оновіть сторінку.");
        RuleFor(command => command.Draft).NotNull().WithErrorCode("invalid_draft")
            .WithMessage("Заповніть чернетку.")
            .SetValidator(new WorkspaceDraftInputValidator());
    }
}

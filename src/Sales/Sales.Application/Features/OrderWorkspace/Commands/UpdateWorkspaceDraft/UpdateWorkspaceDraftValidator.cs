using Sales.Application.Features.OrderWorkspace.Shared;

namespace Sales.Application.Features.OrderWorkspace.Commands.UpdateWorkspaceDraft;

public sealed class UpdateWorkspaceDraftValidator : AbstractValidator<UpdateWorkspaceDraftCommand>
{
    public UpdateWorkspaceDraftValidator()
    {
        RuleFor(command => command.DraftId).GreaterThan(0);
        RuleFor(command => command.Draft).NotNull().WithErrorCode("invalid_draft")
            .WithMessage("Заповніть чернетку.")
            .SetValidator(new WorkspaceDraftInputValidator());
    }
}

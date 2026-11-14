namespace Sales.Application.Features.Orders.AdminWorkspace.Drafts.Update.Validators;

public sealed class UpdateWorkspaceDraftCommandValidator : AbstractValidator<UpdateWorkspaceDraftCommand>
{
    public UpdateWorkspaceDraftCommandValidator()
    {
        RuleFor(command => command.DraftId).GreaterThan(0);
        RuleFor(command => command.Draft).NotNull().WithErrorCode("invalid_draft")
            .WithMessage("Заповніть чернетку.")
            .SetValidator(new WorkspaceDraftInputValidator());
    }
}

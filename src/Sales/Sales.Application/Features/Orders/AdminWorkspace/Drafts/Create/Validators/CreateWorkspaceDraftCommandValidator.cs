namespace Sales.Application.Features.Orders.AdminWorkspace.Drafts.Create.Validators;

public sealed class CreateWorkspaceDraftCommandValidator : AbstractValidator<CreateWorkspaceDraftCommand>
{
    public CreateWorkspaceDraftCommandValidator()
    {
        RuleFor(command => command.IdempotencyKey).NotEmpty().WithErrorCode("invalid_idempotency_key")
            .WithMessage("Оновіть сторінку й збережіть чернетку ще раз.");
        RuleFor(command => command.Draft).NotNull().WithErrorCode("invalid_draft")
            .WithMessage("Заповніть чернетку.")
            .SetValidator(new WorkspaceDraftInputValidator());
    }
}

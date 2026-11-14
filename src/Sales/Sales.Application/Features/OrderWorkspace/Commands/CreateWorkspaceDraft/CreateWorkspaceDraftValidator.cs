using Sales.Application.Features.OrderWorkspace.Shared;

namespace Sales.Application.Features.OrderWorkspace.Commands.CreateWorkspaceDraft;

public sealed class CreateWorkspaceDraftValidator : AbstractValidator<CreateWorkspaceDraftCommand>
{
    public CreateWorkspaceDraftValidator()
    {
        RuleFor(command => command.IdempotencyKey).NotEmpty().WithErrorCode("invalid_idempotency_key")
            .WithMessage("Оновіть сторінку й збережіть чернетку ще раз.");
        RuleFor(command => command.Draft).NotNull().WithErrorCode("invalid_draft")
            .WithMessage("Заповніть чернетку.")
            .SetValidator(new WorkspaceDraftInputValidator());
    }
}

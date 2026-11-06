using Work.Application.Features.Tasks.Common;

namespace Work.Application.Features.Tasks.CreateForUser;

public sealed class CreateWorkTaskForUserCommandValidator : AbstractValidator<CreateWorkTaskForUserCommand>
{
    public CreateWorkTaskForUserCommandValidator()
    {
        RuleFor(x => x.OwnerUserId).NotEmpty();
        WorkTaskInputRules.Title(RuleFor(x => x.Title));
        WorkTaskInputRules.Details(RuleFor(x => x.Details));
        WorkTaskInputRules.Steps(RuleFor(x => x.Steps));
    }
}

using Work.Application.Features.Tasks.Shared;

namespace Work.Application.Features.Tasks.Commands.CreateWorkTaskForUser;

public sealed class CreateWorkTaskForUserValidator : AbstractValidator<CreateWorkTaskForUserCommand>
{
    public CreateWorkTaskForUserValidator()
    {
        RuleFor(x => x.OwnerUserId).NotEmpty();
        WorkTaskInputRules.Title(RuleFor(x => x.Title));
        WorkTaskInputRules.Details(RuleFor(x => x.Details));
        WorkTaskInputRules.Steps(RuleFor(x => x.Steps));
    }
}

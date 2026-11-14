using Work.Application.Features.Tasks.Shared;

namespace Work.Application.Features.Tasks.Commands.CreateMyWorkTask;

public sealed class CreateMyWorkTaskValidator : AbstractValidator<CreateMyWorkTaskCommand>
{
    public CreateMyWorkTaskValidator()
    {
        WorkTaskInputRules.Title(RuleFor(x => x.Title));
        WorkTaskInputRules.Details(RuleFor(x => x.Details));
        WorkTaskInputRules.Steps(RuleFor(x => x.Steps));
    }
}

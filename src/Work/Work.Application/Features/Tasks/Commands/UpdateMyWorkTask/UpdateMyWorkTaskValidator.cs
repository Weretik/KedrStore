using Work.Application.Features.Tasks.Shared;

namespace Work.Application.Features.Tasks.Commands.UpdateMyWorkTask;

public sealed class UpdateMyWorkTaskValidator : AbstractValidator<UpdateMyWorkTaskCommand>
{
    public UpdateMyWorkTaskValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        WorkTaskInputRules.Title(RuleFor(x => x.Title));
        WorkTaskInputRules.Details(RuleFor(x => x.Details));
        WorkTaskInputRules.Steps(RuleFor(x => x.Steps));
    }
}

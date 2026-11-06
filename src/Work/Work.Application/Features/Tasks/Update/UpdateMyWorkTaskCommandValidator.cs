using Work.Application.Features.Tasks.Common;

namespace Work.Application.Features.Tasks.Update;

public sealed class UpdateMyWorkTaskCommandValidator : AbstractValidator<UpdateMyWorkTaskCommand>
{
    public UpdateMyWorkTaskCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        WorkTaskInputRules.Title(RuleFor(x => x.Title));
        WorkTaskInputRules.Details(RuleFor(x => x.Details));
    }
}

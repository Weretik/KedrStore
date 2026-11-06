using Work.Application.Features.Tasks.Common;

namespace Work.Application.Features.Tasks.Create;

public sealed class CreateMyWorkTaskCommandValidator : AbstractValidator<CreateMyWorkTaskCommand>
{
    public CreateMyWorkTaskCommandValidator()
    {
        WorkTaskInputRules.Title(RuleFor(x => x.Title));
        WorkTaskInputRules.Details(RuleFor(x => x.Details));
    }
}

namespace Work.Application.Features.Tasks.Commands.SetMyWorkTaskStep;

public sealed class SetMyWorkTaskStepValidator : AbstractValidator<SetMyWorkTaskStepCommand>
{
    public SetMyWorkTaskStepValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Index).InclusiveBetween(0, WorkTask.StepsMaxCount - 1);
    }
}

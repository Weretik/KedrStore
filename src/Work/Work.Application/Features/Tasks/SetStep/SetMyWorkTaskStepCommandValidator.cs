namespace Work.Application.Features.Tasks.SetStep;

public sealed class SetMyWorkTaskStepCommandValidator : AbstractValidator<SetMyWorkTaskStepCommand>
{
    public SetMyWorkTaskStepCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Index).InclusiveBetween(0, WorkTask.StepsMaxCount - 1);
    }
}

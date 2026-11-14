namespace Scheduling.Application.Features.Tasks.Commands.RunScheduledTaskNow;

public sealed class RunScheduledTaskNowValidator : AbstractValidator<RunScheduledTaskNowCommand>
{
    public RunScheduledTaskNowValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Actor).NotEmpty().MaximumLength(200);
    }
}

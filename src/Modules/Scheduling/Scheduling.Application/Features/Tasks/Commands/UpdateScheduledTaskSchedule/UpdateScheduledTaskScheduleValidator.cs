using Scheduling.Application.Features.Tasks.Shared;

namespace Scheduling.Application.Features.Tasks.Commands.UpdateScheduledTaskSchedule;

public sealed class UpdateScheduledTaskScheduleValidator : AbstractValidator<UpdateScheduledTaskScheduleCommand>
{
    public UpdateScheduledTaskScheduleValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.Schedule).NotNull().SetValidator(new ScheduleInputValidator()).When(x => x.Input is not null);
    }
}

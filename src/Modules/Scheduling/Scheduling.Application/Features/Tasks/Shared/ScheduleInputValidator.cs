namespace Scheduling.Application.Features.Tasks.Shared;

/// <summary>Admin limits of a schedule; the domain keeps its own invariants (platform/007 «HTTP», validators).</summary>
public sealed class ScheduleInputValidator : AbstractValidator<ScheduleInput>
{
    public const int AdminMinIntervalMinutes = 5;

    public ScheduleInputValidator()
    {
        RuleFor(x => x.Kind).Must(x => x is ScheduleInputMapper.Interval or ScheduleInputMapper.Daily or ScheduleInputMapper.Manual)
            .WithMessage("Оберіть, як часто запускати задачу.");

        When(x => x.Kind == ScheduleInputMapper.Interval, () =>
        {
            RuleFor(x => x.EveryMinutes).NotNull().InclusiveBetween(AdminMinIntervalMinutes, ScheduleErrors.MaxIntervalMinutes)
                .WithMessage($"Інтервал має бути від {AdminMinIntervalMinutes} до {ScheduleErrors.MaxIntervalMinutes} хвилин.");
            RuleFor(x => x).Must(x => x.FromHour.HasValue == x.ToHour.HasValue &&
                                      (x.FromHour is null || (x.FromHour is >= 0 and <= 23 && x.ToHour is >= 0 and <= 23 && x.FromHour < x.ToHour)))
                .WithName("fromHour").WithMessage("Години роботи: «з» і «до» від 0 до 23, «з» менше за «до».");
        });

        When(x => x.Kind == ScheduleInputMapper.Daily, () =>
        {
            RuleFor(x => x.Times).NotNull().Must(x => x!.Count is > 0 and <= ScheduleErrors.MaxTimes && x.Distinct().Count() == x.Count)
                .WithMessage($"Вкажіть від 1 до {ScheduleErrors.MaxTimes} різних часів запуску.");
            RuleForEach(x => x.Times).Must(x => ScheduleInputMapper.TryParseTime(x, out _))
                .WithMessage("Час запуску у форматі ГГ:ХХ, наприклад 07:00.");
            RuleFor(x => x.Days).NotNull().Must(x => x!.Count > 0 && x.Distinct().Count() == x.Count)
                .WithMessage("Оберіть хоча б один день тижня.");
            RuleForEach(x => x.Days).InclusiveBetween(1, 7).WithMessage("День тижня від 1 (понеділок) до 7 (неділя).");
        });
    }
}

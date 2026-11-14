using Scheduling.Application.Features.Tasks.Shared;

namespace Scheduling.Application.Features.Tasks.Commands.UpdateScheduledTaskSchedule;

/// <summary>The admin changes the schedule, the on/off switch, or moves the next run once (platform/007 R-02, R-03).</summary>
public sealed class UpdateScheduledTaskScheduleHandler(ISchedulingDbContext db, TimeProvider clock)
    : ICommandHandler<UpdateScheduledTaskScheduleCommand, Result<ScheduledTaskResponse>>
{
    public static DomainError NotEditable { get; } =
        DomainError.Conflict("Scheduling.Task.NotEditable", "Розклад цієї задачі не змінюється: вона працює щохвилини.");

    public async ValueTask<Result<ScheduledTaskResponse>> Handle(UpdateScheduledTaskScheduleCommand command,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var row = await ScheduledTaskRows.FindAsync(db, command.Key, now, cancellationToken);
        if (row is null) return Result<ScheduledTaskResponse>.NotFound();
        if (!row.Definition.Editable) return NotEditable.ToResult<ScheduledTaskResponse>();

        var schedule = ScheduleInputMapper.ToSchedule(command.Input.Schedule);
        if (schedule.IsFailure) return schedule.Error!.ToResult<ScheduledTaskResponse>();
        var changed = row.Task.ChangeSchedule(command.Input.Enabled, schedule.Value, command.Input.NextRunAt, command.Actor, now);
        if (changed.IsFailure) return changed.Error!.ToResult<ScheduledTaskResponse>();

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(ScheduledTaskMapper.ToResponse(row.Definition, row.Task, now));
    }
}

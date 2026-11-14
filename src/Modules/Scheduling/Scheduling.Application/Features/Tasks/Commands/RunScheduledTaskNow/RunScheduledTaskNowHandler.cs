using Scheduling.Application.Features.Tasks.Shared;

namespace Scheduling.Application.Features.Tasks.Commands.RunScheduledTaskNow;

/// <summary>«Запустити зараз» (platform/007 R-08): the next tick, within a minute, starts the task.</summary>
public sealed class RunScheduledTaskNowHandler(ISchedulingDbContext db, TimeProvider clock)
    : ICommandHandler<RunScheduledTaskNowCommand, Result<ScheduledTaskResponse>>
{
    public static DomainError RunsInTick { get; } =
        DomainError.Conflict("Scheduling.Task.RunsInTick", "Ця задача і так працює щохвилини.");

    public async ValueTask<Result<ScheduledTaskResponse>> Handle(RunScheduledTaskNowCommand command,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var row = await ScheduledTaskRows.FindAsync(db, command.Key, now, cancellationToken);
        if (row is null) return Result<ScheduledTaskResponse>.NotFound();
        if (row.Definition.RunsInTick) return RunsInTick.ToResult<ScheduledTaskResponse>();

        var requested = row.Task.RequestRun(command.Actor, now);
        if (requested.IsFailure) return requested.Error!.ToResult<ScheduledTaskResponse>();
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(ScheduledTaskMapper.ToResponse(row.Definition, row.Task, now));
    }
}

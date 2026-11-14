namespace Work.Application.Features.Activity.Record;

public sealed class RecordWorkActivityCommandHandler(
    IWorkRepository<WorkActivity> activities, IReadWorkDbContext db, TimeProvider clock)
    : ICommandHandler<RecordWorkActivityCommand, Result>
{
    public async ValueTask<Result> Handle(RecordWorkActivityCommand command, CancellationToken cancellationToken)
    {
        var recorded = WorkActivity.Record(command.UserId, command.UserEmail, command.Area, command.Operation,
            command.Method, command.Target, clock.GetUtcNow());
        if (recorded.IsFailure)
            return recorded.Error!.ToResult();
        var next = recorded.Value;

        // R-6: the user's latest entry absorbs a repeat of the same change. The read context is the
        // repository's DbContext, so the entry read here is tracked for the update.
        var last = await db.WorkActivities
            .Where(activity => activity.UserId == next.UserId)
            .OrderByDescending(activity => activity.OccurredAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (last is not null && last.CanMerge(next))
        {
            last.Repeat(next.OccurredAt);
            await activities.UpdateAsync(last, cancellationToken);
        }
        else
        {
            await activities.AddAsync(next, cancellationToken);
        }

        return Result.Success();
    }
}

using Scheduling.Application.Features.Catalog;

namespace Scheduling.Application.Features.Tasks.Shared;

/// <summary>A catalogue task with its row; commands create a missing row with the catalogue defaults.</summary>
public sealed record ScheduledTaskRow(ScheduledTaskDefinition Definition, ScheduledTask Task);

public static class ScheduledTaskRows
{
    /// <summary>Every catalogue task in catalogue order; rows of keys no longer in the catalogue are left out.</summary>
    public static async Task<IReadOnlyList<ScheduledTaskRow>> LoadAsync(ISchedulingDbContext db, DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var saved = await db.Tasks.ToDictionaryAsync(x => x.Key, StringComparer.Ordinal, cancellationToken);
        return ScheduledTaskCatalog.Tasks.Select(definition => new ScheduledTaskRow(definition,
            Synced(definition, saved.GetValueOrDefault(definition.Key), now) ?? Create(db, definition, now))).ToArray();
    }

    /// <summary>
    /// Every catalogue task for reading only: rows are read without tracking, a missing row is shown with the catalogue
    /// defaults and a fixed task with the catalogue schedule, and nothing is written (the scheduler tick saves them).
    /// </summary>
    public static async Task<IReadOnlyList<ScheduledTaskRow>> ReadAsync(ISchedulingDbContext db, DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var saved = await db.Tasks.AsNoTracking().ToDictionaryAsync(x => x.Key, StringComparer.Ordinal, cancellationToken);
        return ScheduledTaskCatalog.Tasks.Select(definition => new ScheduledTaskRow(definition,
            Synced(definition, saved.GetValueOrDefault(definition.Key), now) ?? Defaults(definition, now))).ToArray();
    }

    /// <summary>One catalogue task, or null when the key is unknown.</summary>
    public static async Task<ScheduledTaskRow?> FindAsync(ISchedulingDbContext db, string key, DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var definition = ScheduledTaskCatalog.Find(key);
        if (definition is null) return null;
        var task = Synced(definition, db.Tasks.Local.FirstOrDefault(x => x.Key == key)
                   ?? await db.Tasks.FirstOrDefaultAsync(x => x.Key == key, cancellationToken), now)
                   ?? Create(db, definition, now);
        return new ScheduledTaskRow(definition, task);
    }

    /// <summary>A task the Admin cannot edit always follows the catalogue schedule, also after the catalogue changes.</summary>
    private static ScheduledTask? Synced(ScheduledTaskDefinition definition, ScheduledTask? task, DateTimeOffset now)
    {
        if (task is not null && !definition.Editable && !task.Schedule.Equals(definition.DefaultSchedule))
            _ = task.ChangeSchedule(task.Enabled, definition.DefaultSchedule, null, SystemActor, now);
        return task;
    }

    private const string SystemActor = "system";

    private static ScheduledTask Create(ISchedulingDbContext db, ScheduledTaskDefinition definition, DateTimeOffset now)
    {
        var task = Defaults(definition, now);
        db.Tasks.Add(task);
        return task;
    }

    private static ScheduledTask Defaults(ScheduledTaskDefinition definition, DateTimeOffset now) =>
        ScheduledTask.Create(definition.Key, definition.DefaultSchedule, definition.EnabledByDefault, now);
}

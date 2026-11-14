using Scheduling.Application.Features.Catalog;

namespace Scheduling.Application.Features.Tasks.Shared;

/// <summary>A catalogue task with its row; the row is created with the catalogue defaults when it is missing.</summary>
public sealed record ScheduledTaskRow(ScheduledTaskDefinition Definition, ScheduledTask Task);

public static class ScheduledTaskRows
{
    /// <summary>Every catalogue task in catalogue order; rows of keys no longer in the catalogue are left out.</summary>
    public static async Task<IReadOnlyList<ScheduledTaskRow>> LoadAsync(ISchedulingDbContext db, DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var saved = await db.Tasks.ToDictionaryAsync(x => x.Key, StringComparer.Ordinal, cancellationToken);
        return ScheduledTaskCatalog.Tasks.Select(definition => new ScheduledTaskRow(definition,
            saved.GetValueOrDefault(definition.Key) ?? Create(db, definition, now))).ToArray();
    }

    /// <summary>One catalogue task, or null when the key is unknown.</summary>
    public static async Task<ScheduledTaskRow?> FindAsync(ISchedulingDbContext db, string key, DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var definition = ScheduledTaskCatalog.Find(key);
        if (definition is null) return null;
        var task = db.Tasks.Local.FirstOrDefault(x => x.Key == key)
                   ?? await db.Tasks.FirstOrDefaultAsync(x => x.Key == key, cancellationToken)
                   ?? Create(db, definition, now);
        return new ScheduledTaskRow(definition, task);
    }

    private static ScheduledTask Create(ISchedulingDbContext db, ScheduledTaskDefinition definition, DateTimeOffset now)
    {
        var task = ScheduledTask.Create(definition.Key, definition.DefaultSchedule, definition.EnabledByDefault, now);
        db.Tasks.Add(task);
        return task;
    }
}

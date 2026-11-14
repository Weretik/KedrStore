using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scheduling.Contracts.Runs;

namespace Host.Jobs.Tasks;

/// <summary>
/// <c>--job=tasks --tasks=key:runId,...</c> (spec platform/007 R-06): the tick's claimed tasks one after another, each in
/// its own scope. A stale run is skipped; one failure does not stop the next task; the exit code is 1 when any failed.
/// </summary>
public static class ScheduledTasksJobRunner
{
    public const string Argument = "--tasks";

    public static IReadOnlyList<(string Key, Guid RunId)> Parse(string? value) =>
        (value ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(item => item.Split(':', 2))
            .Select(parts => parts.Length == 2 && Guid.TryParse(parts[1], out var runId)
                ? (parts[0], runId)
                : throw new ArgumentException($"{Argument}: «{string.Join(':', parts)}» is not key:runId."))
            .ToArray();

    public static async Task<int> RunAsync(IServiceProvider services, string? value, ILogger logger,
        CancellationToken cancellationToken)
    {
        var failed = 0;
        foreach (var (key, runId) in Parse(value))
        {
            await using var scope = services.CreateAsyncScope();
            var journal = scope.ServiceProvider.GetRequiredService<IScheduledTaskRunJournal>();
            if (!await journal.StartAsync(key, runId, cancellationToken)) continue;

            var (succeeded, message) = await RunOneAsync(key, scope.ServiceProvider, logger, cancellationToken);
            if (!succeeded) failed++;
            await using var finishScope = services.CreateAsyncScope();
            await finishScope.ServiceProvider.GetRequiredService<IScheduledTaskRunJournal>()
                .FinishAsync(key, runId, succeeded, message, CancellationToken.None);
        }

        return failed == 0 ? 0 : 1;
    }

    private static async Task<(bool Succeeded, string Message)> RunOneAsync(string key, IServiceProvider services,
        ILogger logger, CancellationToken cancellationToken)
    {
        var runner = ScheduledTaskRunners.Find(key);
        if (runner is null) return (false, $"Задачу «{key}» не знайдено в Host.Jobs.");
        try
        {
            logger.LogInformation("Task {Task} started", key);
            var summary = await runner.RunAsync(services, cancellationToken);
            logger.LogInformation("Task {Task} finished: {Summary}", key, summary);
            return (true, summary);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Task {Task} failed", key);
            return (false, exception.GetBaseException().Message);
        }
    }
}

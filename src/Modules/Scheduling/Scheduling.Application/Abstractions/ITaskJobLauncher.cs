namespace Scheduling.Application.Abstractions;

/// <summary>One task the job must run, with the run the tick claimed for it.</summary>
public sealed record TaskLaunch(string Key, Guid RunId);

/// <summary>Starts one execution of the tasks job (kedr-tasks) for the claimed tasks (platform/007 R-04 e).</summary>
public interface ITaskJobLauncher
{
    /// <summary>Null when the execution started; otherwise the reason the admin sees on each task (R-05).</summary>
    Task<string?> LaunchAsync(IReadOnlyList<TaskLaunch> launches, CancellationToken cancellationToken);
}

namespace Host.Jobs.Tasks;

/// <summary>
/// The work of one task of «Фонові задачі» (spec platform/007 R-06): the key matches the Scheduling catalogue; the
/// answer is the short Ukrainian summary the admin sees; an exception is the failure and its message the reason.
/// </summary>
public interface IScheduledTaskRunner
{
    string Key { get; }

    Task<string> RunAsync(IServiceProvider services, CancellationToken cancellationToken);
}

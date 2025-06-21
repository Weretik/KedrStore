namespace Application.Interfaces
{
    public interface IBackgroundJobService
    {
        string Enqueue<T>(object args = null, JobOptions options = null) where T : IJobHandler;
        string Schedule<T>(TimeSpan delay, object args = null, JobOptions options = null) where T : IJobHandler;

        string Schedule<T>(DateTime enqueueAt, object args = null, JobOptions options = null) where T : IJobHandler;

        string RecurringJob<T>(string cronExpression, object args = null, JobOptions options = null) where T : IJobHandler;

        bool Delete(string jobId);

        bool Exists(string jobId);

        bool Trigger(string jobId);
    }
}

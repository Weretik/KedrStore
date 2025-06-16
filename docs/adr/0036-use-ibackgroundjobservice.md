# ADR 0036: Use IBackgroundJobService

## Date
2025-06-17

## Status
Accepted

## Context
Background jobs are essential for handling tasks that do not need to be executed immediately or synchronously, such as sending emails, processing large datasets, or generating reports. `IBackgroundJobService` provides a clean abstraction for managing background jobs, enabling flexibility to use various job scheduling libraries such as Hangfire, Quartz.NET, or custom implementations.

## Decision
We decided to use `IBackgroundJobService` in the project to:

1. Provide a centralized and consistent way to manage background jobs.
2. Decouple the application layer from specific job scheduling libraries.
3. Enable flexibility to switch between job scheduling libraries without affecting the application logic.
4. Align with best practices for clean and maintainable architecture.

## Consequences
### Positive
1. Improves maintainability by centralizing background job logic.
2. Decouples the application layer from specific job scheduling libraries.
3. Simplifies testing by allowing mock implementations of `IBackgroundJobService`.
4. Enhances scalability by supporting various job scheduling libraries.

### Negative
1. Adds complexity by introducing an additional abstraction layer.
2. Requires careful implementation to ensure reliability and consistency.

## Example
`IBackgroundJobService` is implemented as follows:

**IBackgroundJobService.cs**:
```csharp
public interface IBackgroundJobService
{
    string Enqueue<T>(object args = null, JobOptions options = null) where T : IJobHandler;
    string Schedule<T>(TimeSpan delay, object args = null, JobOptions options = null) where T : IJobHandler;
    string RecurringJob<T>(string cronExpression, object args = null, JobOptions options = null) where T : IJobHandler;
}
```

**Usage in Application Layer**:
```csharp
public class NotificationService
{
    private readonly IBackgroundJobService _backgroundJobService;

    public NotificationService(IBackgroundJobService backgroundJobService)
    {
        _backgroundJobService = backgroundJobService;
    }

    public void ScheduleEmailNotification(string email, string subject, string body)
    {
        _backgroundJobService.Enqueue<EmailNotificationJob>(new { email, subject, body });
    }
}
```

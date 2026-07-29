# ADR 0038: Use PerformanceBehavior

## Status

Accepted

## Date

2025-06-20

## Context

In the application, we aim to ensure optimal performance and responsiveness of our CQRS pipeline. To achieve this, we need a mechanism to measure and log the execution time of requests and commands processed by the pipeline.

## Decision

We decided to use the `PerformanceBehavior` as part of the MediatR pipeline. This behavior will measure the execution time of each request and log it if it exceeds a predefined threshold.

## Consequences

### Positive Consequences

- Helps identify performance bottlenecks in the application.
- Provides valuable insights into the execution time of requests, enabling developers to optimize slow operations.
- Improves the overall responsiveness of the application.

### Negative Consequences

- Adds additional logging, which may slightly increase the size of log files.
- Requires careful configuration of the threshold to avoid excessive logging.
- May introduce minor overhead in the pipeline due to performance measurement.

The `PerformanceBehavior` will help identify performance bottlenecks in the application. It will provide valuable insights into the execution time of requests, enabling developers to optimize slow operations. The behavior is integrated into the pipeline via the `AddApplicationServices` extension method in the `ApplicationServiceCollection` class.

## Implementation

The `PerformanceBehavior` is registered in the MediatR pipeline as follows:

```csharp
cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
```

This ensures that every request passing through the pipeline is monitored for performance metrics.

## Example Usage

The `PerformanceBehavior` is implemented in the MediatR pipeline as follows:

```csharp
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly Stopwatch _timer;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
        _timer = new Stopwatch();
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        if (_timer.ElapsedMilliseconds > 500) // Example threshold
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogWarning($"Long Running Request: {requestName} ({_timer.ElapsedMilliseconds} milliseconds)");
        }

        return response;
    }
}
```

This example demonstrates how the `PerformanceBehavior` measures execution time and logs warnings for long-running requests.

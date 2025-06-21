# ADR 0046: Use LoggingService

## Date
2025-06-21

## Status
Accepted

## Context
The `LoggingService` is used to standardize logging operations across the application. It provides methods for logging validation processes, request lifecycle events, exceptions, and performance metrics. This ensures consistent and structured logging.

## Decision
We decided to use the `LoggingService` for all logging operations instead of directly using `ILogger`.

## Examples
### Positive Example
```csharp
var loggingService = new LoggingService(logger);
loggingService.LogRequestStarted("CreateOrder", userId, payload);
loggingService.LogRequestSucceeded("CreateOrder", elapsedMs, userId, response);
```

### Negative Example
```csharp
logger.LogInformation("➡️ CreateOrder started | User: {UserId} | Payload: {@Payload}", userId, payload);
logger.LogInformation("✅ CreateOrder succeeded in {Elapsed}ms | User: {UserId} | Response: {@Response}", elapsedMs, userId, response);
```

## Consequences
### Positive
- Centralized and consistent logging.
- Improves readability and maintainability of logging code.
- Provides structured methods for common logging scenarios.

### Negative
- Adds dependency on the `LoggingService`.
- Requires developers to use the service instead of direct `ILogger` operations.

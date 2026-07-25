# ADR 0045: Use EnvironmentService

## Date
2025-06-21

## Status
Accepted

## Context
The `EnvironmentService` is used to determine the current environment in which the application is running. It provides methods to check if the application is running in production mode, which is useful for environment-specific configurations and behaviors.

## Decision
We decided to use the `EnvironmentService` for environment checks instead of directly using `IWebHostEnvironment`.

## Examples
### Positive Example
```csharp
var environmentService = new EnvironmentService(env);
if (environmentService.IsProduction())
{
    // Production-specific logic
}
```

### Negative Example
```csharp
if (env.IsProduction())
{
    // Production-specific logic
}
```

## Consequences
### Positive
- Centralized environment checks.
- Improves testability by abstracting `IWebHostEnvironment`.
- Consistent behavior across the application.

### Negative
- Adds dependency on the `EnvironmentService`.
- Requires developers to use the service instead of direct environment checks.

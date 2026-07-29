# ADR 0043: Use DateTimeProvider Service

## Date
2025-06-21

## Status
Accepted

## Context
The `DateTimeProvider` service is used to abstract the handling of date and time operations in the application. It provides methods for retrieving the current UTC time, converting time zones, and calculating time zone offsets. This ensures consistency and testability across the application.

## Decision
We decided to use the `DateTimeProvider` service for all date and time-related operations instead of directly using `DateTime` or `TimeZoneInfo`.

## Examples
### Positive Example
```csharp
var dateTimeProvider = new DateTimeProvider();
var utcNow = dateTimeProvider.UtcNow;
var localTime = dateTimeProvider.GetCurrentTime("Pacific Standard Time");
```

### Negative Example
```csharp
var utcNow = DateTime.UtcNow;
var tz = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);
```

## Consequences
### Positive
- Centralized handling of date and time operations.
- Improved testability by allowing mocking of date and time.
- Consistent behavior across the application.

### Negative
- Adds a slight overhead of dependency injection.
- Requires developers to use the service instead of direct `DateTime` operations.

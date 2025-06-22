# Logging Strategy – KedrStore

## Overview

This document describes the production-grade logging architecture implemented in the KedrStore project. It explains how logging is structured across all layers of the application, what tools are used, and how it can be extended.

---

## Goals

- Centralize and standardize logging across the system
- Ensure traceability of actions across layers (UI → App → Infra)
- Prevent technical leaks to UI
- Enable structured logging for monitoring (e.g., Seq, ELK)
- Support correlation of logs via Correlation ID

---

## Technology Stack

| Area         | Tool            |
|--------------|-----------------|
| Logging      | Serilog         |
| Abstraction  | ILoggingService |
| Middleware   | Custom middleware (CorrelationIdMiddleware) |
| Output       | Rolling log files (TXT/JSON) |
| Enrichment   | LogContext + Enrichers (ThreadId, MachineName, etc.) |

---

## Layered Logging Responsibilities

```text
┌────────────────────────────┐
│  Presentation (UI Layer)  │
│  • ErrorBoundary           │
│  • ILoggingService (UI)    │
└────────────┬───────────────┘
             │
             ▼
┌────────────────────────────┐
│  Application Layer         │
│  • LoggingBehavior<T>      │
│  • ExceptionBehavior<T>    │
└────────────┬───────────────┘
             │
             ▼
┌────────────────────────────┐
│  Infrastructure Layer      │
│  • LoggingService          │
│  • Logs external failures  │
└────────────┬───────────────┘
             │
             ▼
┌────────────────────────────┐
│  Middleware Layer           │
│  • CorrelationIdMiddleware │
└────────────────────────────┘
```

---

## Key Components

### 1. ILoggingService

A custom abstraction over `ILogger<T>`, injected across all layers:

```csharp
public interface ILoggingService
{
    void LogInfo(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(Exception exception, string message, params object[] args);
    void LogDebug(string message, params object[] args);
}
```

### 2. LoggingBehavior

Logs all requests and response durations:

```csharp
_logger.LogInformation("Handling {RequestName} {@Request}", typeof(TRequest).Name, request);
// ...
_logger.LogInformation("Handled {RequestName} in {Elapsed}ms", typeof(TRequest).Name, stopwatch.ElapsedMilliseconds);
```

### 3. ExceptionHandlingBehavior

Catches and logs unexpected exceptions in the pipeline and wraps them in `AppError`:

```csharp
_logger.LogError(exception, "Unhandled exception for {RequestName}", typeof(TRequest).Name);
return AppResult.Failure<TResponse>(AppError.Unexpected("Internal error"));
```

### 4. CorrelationIdMiddleware

Middleware that injects `X-Correlation-ID` into requests:

```csharp
using (LogContext.PushProperty("CorrelationId", correlationId))
{
    await _next(context);
}
```

Registered in `Program.cs`:
```csharp
app.UseMiddleware<CorrelationIdMiddleware>();
```

---

## Serilog Configuration (appsettings.json)

```json
"Serilog": {
  "MinimumLevel": "Information",
  "WriteTo": [
    {
      "Name": "File",
      "Args": {
        "path": "Logs/log-.txt",
        "rollingInterval": "Day",
        "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}"
      }
    }
  ],
  "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ]
}
```

---

## Best Practices

- Use structured logging (`{PropertyName}`) everywhere
- Avoid logging sensitive data (e.g., passwords, tokens)
- Enrich logs with context (user, tenant, correlation ID)
- Do not throw exceptions for expected flows (use AppResult)
- Register all custom logging behaviors via DI

---

## Future Enhancements

| Feature                     | Status     | Notes                                  |
|----------------------------|------------|----------------------------------------|
| JSON log output            | Pending    | Add `CompactJsonFormatter` for Seq     |
| Correlation ID propagation | In Progress| Middleware exists, add context accessor|
| Alerts via sink (Telegram) | Planned    | Use Serilog.Sinks.Telegram             |
| Centralized UI errors      | Planned    | Blazor-wide error display              |

---

## Summary

The KedrStore logging system is production-ready, based on clean separation of concerns, structured output, and layered design. It can be extended to support full observability with minimal changes.


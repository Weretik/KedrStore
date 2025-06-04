# ADR 0004: Use Serilog for Structured Logging

## Status
Accepted

## Date
2025-06-04

## Context
Logging is a critical infrastructure concern in KedrStore. It must support:

- Structured output for log analysis and correlation
- Centralized log collection (file, console, Seq, etc.)
- Filtering by level (e.g. Information, Warning, Error)
- Enrichment with request context, correlation IDs, and exception details

ASP.NET Core provides a built-in logging abstraction via `ILogger<T>`, and supports third-party providers like Serilog, NLog, etc.

**Serilog** was selected due to:

- Rich ecosystem of sinks (File, Console, Seq, Elasticsearch, etc.)
- Built-in support for structured logging
- Seamless integration with ASP.NET Core DI and configuration
- Enrichment capabilities (e.g. with user ID, correlation ID, environment)

## Decision
KedrStore will use **Serilog** as the main logging library. It will be registered during application startup via `UseSerilog()`.

Logging configuration will be placed in `appsettings.json` and support multiple sinks (e.g. file + console for dev, Seq for staging/prod).

Structured logs will include:
- Timestamps and levels
- Request IDs and correlation tokens
- Exception details with stack trace
- Application name and environment

## Consequences

### Positive
- Flexible and powerful logging configuration
- Machine-readable logs (JSON)
- Enrichers provide automatic contextual data
- Scalable to cloud, file, or remote sinks

### Negative
- Slightly steeper learning curve than default logging
- Requires sink setup (e.g. install Seq or configure Elastic)


# ADR-0006: Business rules return a Result with a typed error; exceptions only for the unexpected

- Status: accepted
- Date: 2026-11-15 (recorded from the current code; rule codified on 2026-11-14)
- Owner: Виталий (project owner)

## Context

Business failures (a product not found, a price type in use, a schedule in the past) are normal outcomes, not crashes. Earlier code threw exceptions for them and caught them in endpoints, so status codes were chosen in many places and some answers leaked exception text. Staff read the messages in Ukrainian.

## Alternatives considered

- Exceptions for business rules plus a mapping middleware: rejected; control flow is hidden, and an uncaught case becomes a 500.
- Each endpoint picks its own status code: rejected; the same error got different codes in different modules.

## Decision

- Domain and Application return `Ardalis.Result` (`Result` / `Result<T>`) with a typed error from the aggregate's `Errors` (code plus Ukrainian message). They never reference HTTP types.
- Every input is checked by a FluentValidation validator beside its use case, with Ukrainian messages; the validation behavior returns `Invalid` before the handler runs.
- One mapper (`BuildingBlocks.Api/Results/ResultHttp.ToHttpResult` / `ToProblem`) turns every result into a response; failures are `application/problem+json`.
- Exceptions are only for programmer errors and the unexpected. One `GlobalExceptionHandler` logs them and answers a problem response without exception text or stack trace.

## Why

- A reader of a handler sees every failure it can return.
- The same error has the same status code everywhere.
- A client never sees internal details.

## Trade-offs

- `Result` adds a check after every call that can fail.
- Use cases without validators still exist and are listed as known deviations in the [module rules](../../standards/module-rules.md).

## Sources and checks

- `src/BuildingBlocks/BuildingBlocks.Api/Results/ResultHttp.cs`, `src/BuildingBlocks/BuildingBlocks.Api/Errors/GlobalExceptionHandler.cs`.
- [API rules](../../standards/api-rules.md), [backend rules](../../standards/backend-rules.md).

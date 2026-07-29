# Mediator behaviors

The host registers these pipeline behaviors for all mediator requests in this exact order:

~~~text
ISender.Send(...)
  │
  ├── 1. RequestLoggingBehavior
  ├── 2. ExceptionBehavior
  ├── 3. PerformanceBehavior
  ├── 4. ValidationBehavior
  ├── 5. DomainEventDispatcherBehavior
  └── Handler
~~~

## 1. RequestLoggingBehavior

Logs the start, completion and failure context of a mediator request using the existing structured logging conventions. It is observability only; it must not change business behaviour or log sensitive request payloads.

## 2. ExceptionBehavior

Provides the established exception handling path around a request. Expected business outcomes must still be represented by Ardalis.Result, not thrown as exceptions.

## 3. PerformanceBehavior

Measures request duration using the configured PerformanceBehavior options and emits diagnostics for slow requests. It does not alter query/command semantics.

## 4. ValidationBehavior

Runs all FluentValidation validators for the request before the handler. For Result or Result<T> responses, validation failures become Result.Invalid; the handler is not called. Validators check input contracts, not aggregate state rules.

## 5. DomainEventDispatcherBehavior

Runs after the handler succeeds. It obtains collected events from IDomainEventContext, clears them, and publishes each through IDomainEventDispatcher/Mediator. Entities collect events; controllers and handlers do not bypass this lifecycle.

## Rules for new behavior

- Add a behavior only for a truly cross-cutting concern that applies to multiple use cases.
- Preserve the order deliberately; document any order change in an ADR and affected feature specification.
- Never put domain policy, endpoint-specific authorization or use-case orchestration into a behavior.

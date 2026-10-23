# Cross-cutting Mediator behaviors

The host registers these behaviors for all Mediator requests in exactly this order:

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

Logs the start, completion, and failure context of a Mediator request under the current structured logging rules. This is observability only: the behavior does not change business behavior or log sensitive request payloads.

## 2. ExceptionBehavior

Provides the current exception handling path around the request. Expected business outcomes continue to use Ardalis.Result rather than exceptions.

## 3. PerformanceBehavior

Measures request duration according to the configured PerformanceBehavior settings and records diagnostics for slow requests. It does not change query/command semantics.

## 4. ValidationBehavior

Runs all FluentValidation validators for the request before the handler. For `Result` or `Result<T>` responses, failures become `Result.Invalid`, and the handler is not called. Validators check input contracts, not aggregate state rules.

## 5. DomainEventDispatcherBehavior

Runs after a successful handler. It retrieves accumulated events from `IDomainEventContext`, clears them, and publishes each through `IDomainEventDispatcher`/Mediator. Entities accumulate events; controllers and handlers do not bypass this cycle.

## Rules for a new behavior

- Add a behavior only for a genuinely cross-cutting need shared by multiple use cases.
- Preserve the order deliberately; document an order change in an [ADR](../decisions/README.md) and the affected feature specification.
- Do not put a domain rule, endpoint-specific authorization, or use-case coordination in a pipeline behavior.

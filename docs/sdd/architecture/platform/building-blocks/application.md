# BuildingBlocks.Application

## Structure

~~~text
BuildingBlocks.Application/
├── Behaviors/
│   ├── RequestLoggingBehavior
│   ├── ExceptionBehavior
│   ├── PerformanceBehavior + options
│   ├── ValidationBehavior
│   └── DomainEventDispatcherBehavior
├── Contracts/
│   ├── ICurrentUserService, IPermissionService
│   ├── ICacheService, IEmailService, IEnvironmentService
│   └── technical abstractions for modules
├── Notifications/
│   ├── IDomainEventContext, IDomainEventDispatcher
│   └── DomainEventNotification + handler
├── Logging/                  generated structured log helpers
├── Helpers/PhoneNumberHelper.cs
└── ApplicationAssemblyMarker.cs
~~~

## Pipeline behaviors

The host registers RequestLogging, Exception, Performance, Validation, and DomainEventDispatcher in exactly this order. See [Mediator behaviors](../../runtime/mediator-behaviors.md) for their precise semantics.

## Notifications

`IDomainEventContext` provides entities with accumulated events. `IDomainEventDispatcher` publishes events. `DomainEventNotification` is a Mediator notification wrapper. The default notification handler is intentionally empty; handlers in specific modules can subscribe to this notification/event pattern when needed.

## Logging and helpers

The Logging folder contains stable source-generated Serilog/Microsoft templates for requests, validation, performance, domain events, cancellation, and unhandled failures. Use them through the existing behaviors; do not log raw sensitive request data.

`PhoneNumberHelper` is a shared utility for normalizing and validating phone numbers. Use it instead of duplicating parsing logic.

## Contract availability

`ICurrentUserService` and `IPermissionService` have shared implementations in Infrastructure. `ICacheService`, `IEmailService`, and `IEnvironmentService` are currently only contracts in BuildingBlocks; no shared implementation is registered there. Do not inject them into a new feature until the responsible module provides and registers an implementation.

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
│   └── module-facing technical abstractions
├── Notifications/
│   ├── IDomainEventContext, IDomainEventDispatcher
│   └── DomainEventNotification + handler
├── Logging/                  source-generated structured log helpers
├── Helpers/PhoneNumberHelper.cs
└── ApplicationAssemblyMarker.cs
~~~

## Behaviors

The host registers RequestLogging, Exception, Performance, Validation and DomainEventDispatcher in that order. Read [Mediator behaviors](../../runtime/mediator-behaviors.md) for the exact semantics.

## Notifications

IDomainEventContext exposes entities with collected events. IDomainEventDispatcher publishes events. DomainEventNotification is the Mediator notification wrapper. The default notification handler is intentionally empty; module-specific handlers subscribe to the notification/event pattern where needed.

## Logging and helper approach

The Logging folder contains stable, source-generated Serilog/Microsoft logging templates for requests, validation, performance, domain events, cancellation and unhandled failures. Use them through existing behaviors; do not log raw sensitive request data.

PhoneNumberHelper is the shared phone-normalisation/validation utility. Reuse it instead of duplicating phone parsing.

## Contract availability

ICurrentUserService and IPermissionService have shared Infrastructure implementations. ICacheService, IEmailService and IEnvironmentService are contracts only in BuildingBlocks at present; no shared implementation is registered there. Do not inject them into a new feature until an owning module supplies and registers an implementation.

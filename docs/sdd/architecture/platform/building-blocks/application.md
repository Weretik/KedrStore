# BuildingBlocks.Application

## Structure

~~~text
BuildingBlocks.Application/
├── Behaviors/
│   ├── RequestLoggingBehavior
│   ├── ExceptionBehavior
│   ├── PerformanceBehavior + options
│   └── ValidationBehavior
├── Contracts/
│   ├── ICurrentUserService, IPermissionService
│   ├── ICacheService, IEmailService, IEnvironmentService
│   └── technical abstractions for modules
├── Logging/                  generated structured log helpers
├── Helpers/PhoneNumberHelper.cs
└── ApplicationAssemblyMarker.cs
~~~

## Pipeline behaviors

The host registers RequestLogging, Exception, Performance, and Validation in exactly this order. See [Mediator behaviors](../../runtime/mediator-behaviors.md) for their precise semantics.

## Logging and helpers

The Logging folder contains stable source-generated Serilog/Microsoft templates for requests, validation, performance, cancellation, and unhandled failures. Use them through the existing behaviors; do not log raw sensitive request data.

`PhoneNumberHelper` is a shared utility for normalizing and validating phone numbers. Use it instead of duplicating parsing logic.

## Contract availability

`ICurrentUserService` and `IPermissionService` have shared implementations in Infrastructure. `ICacheService`, `IEmailService`, and `IEnvironmentService` are currently only contracts in BuildingBlocks; no shared implementation is registered there. Do not inject them into a new feature until the responsible module provides and registers an implementation.

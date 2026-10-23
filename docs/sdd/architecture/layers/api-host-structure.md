# API and host structure

## API module structure

~~~text
<Module>.Api/
├── Controllers/
│   └── <Area>Controller.cs
├── GlobalUsing.cs
└── <Module>.Api.csproj
~~~

Controllers bind HTTP input, call `ISender.Send` with `CancellationToken`, and return the result through the existing Result mapping. They do not run EF queries, create repositories, or contain domain rules.

## Host structure

~~~text
Bootstrapper/Host.Api/
├── Program.cs
├── DependencyInjection/
│   ├── ServiceRegistration/
│   │   ├── ApplicationRegistrationExtensions.cs
│   │   ├── Pipeline/
│   │   ├── SecurityRegistrationExtensions.cs
│   │   └── Web/
│   └── WebApplication/
└── appsettings*.json
~~~

## Request pipeline

~~~text
HTTP
  → exception handling / Development diagnostics
  → CORS, HTTPS, Serilog request logging, rate limiter
  → authentication → authorization
  → controller → ISender
  → request logging → exceptions → performance → validation → domain events
  → handler → Result to HTTP mapping
~~~

The host is the composition root. It registers modules and middleware but does not implement business use cases. `Host.Api` does not run migrations at startup; a separate [migration job](../../operations/data/migrations-and-seeding.md) handles them.

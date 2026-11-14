# API and host structure

## API module structure

~~~text
<Module>.Api/
├── Endpoints/
│   └── <Feature>/
│       ├── <Feature>Endpoints.cs           Map<Feature>Endpoints(): one MapGroup per audience (admin, public, mobile) with tag and policy
│       ├── <UseCase>Endpoint.cs            Map(group) + Handle: one operation
│       └── <UseCase>Request.cs             HTTP request record of the feature (≠ the Application command)
├── DependencyInjection/
│   └── <Module>Module.cs                   Add<Module>Module(): Application, Infrastructure; Map<Module>Endpoints()
└── <Module>.Api.csproj
~~~

Endpoints bind HTTP input into a request record, call `ISender.Send` with `CancellationToken`, and answer through the one Result mapping (`ToHttpResult`, failures as `ProblemDetails`). They do not run EF queries, create repositories, use Infrastructure types, or contain domain rules. Groups and endpoints take policy names from `Identity.Contracts`.

## Host structure

~~~text
Bootstrapper/Host.Api/
├── Program.cs
├── DependencyInjection/
│   ├── ServiceRegistration/
│   │   ├── ModulesRegistrationExtensions.cs   calls Add<Module>Module() for every module
│   │   ├── Pipeline/
│   │   ├── SecurityRegistrationExtensions.cs
│   │   └── Web/
│   └── WebApplication/
└── appsettings*.json

Bootstrapper/Host.Jobs/
├── Program.cs                                 parses --job and runs it
└── JobsHostServicesExtensions.cs              registers modules; job logic lives in the owning module
~~~

## Request pipeline

~~~text
HTTP
  → exception handling / Development diagnostics
  → CORS, HTTPS, Serilog request logging, rate limiter
  → authentication → authorization
  → Work activity log (admin changes) → endpoint → ISender
  → request logging → exceptions → performance → validation → domain events
  → handler → Result to HTTP mapping
~~~

The host is the composition root. It registers modules and middleware but does not implement business use cases. `Host.Api` does not run migrations at startup; a separate [migration job](../../operations/data/migrations-and-seeding.md) handles them.

# API and host structure

## API module structure

~~~text
<Module>.Api/
├── Endpoints/
│   └── <Feature>/
│       ├── <Feature>Endpoints.cs           Map<Feature>Endpoints(): one MapGroup per audience (admin, public, mobile) with tag and policy
│       ├── <UseCase>/                      <UseCase>Endpoint.cs (Map + Handle) with its HTTP request and response records (≠ the Application command)
│       └── Shared/                         records or mapping used by two or more endpoints of the feature
├── DependencyInjection/
│   └── <Module>Module.cs                   Add<Module>Module(): Application, Infrastructure; Map<Module>Endpoints()
└── <Module>.Api.csproj
~~~

Endpoints bind HTTP input into a request record, call `ISender.Send` with `CancellationToken`, and answer through the one Result mapping (`ToHttpResult`, failures as `ProblemDetails`). They do not run EF queries, create repositories, use Infrastructure types, or contain domain rules. Groups and endpoints take policy names from `Identity.Contracts`.

## Host structure

~~~text
Bootstrapper/Host.Api/
├── Program.cs
├── HostServicesExtensions.cs                  AddHostServices(): the topics below in order
├── ModulesRegistrationExtensions.cs           calls Add<Module>Module() for every module
├── SerilogExtensions.cs
├── Application/                               Mediator and the Application pipeline
├── Http/                                      Minimal API, OpenAPI, mobile sync compression
├── Network/                                   forwarded headers, rate limits; TrustedProxy/
├── Security/                                  authentication, authorization policies, CORS; Csrf/
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

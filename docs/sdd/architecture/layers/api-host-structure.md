# API and host structure

## API module structure

~~~text
<Module>.Api/
├── Features/
│   └── <Feature>/
│       ├── Admin<Feature>Controller.cs     one controller per audience (admin, public, mobile)
│       ├── Public<Feature>Controller.cs
│       └── <Action>Request.cs              HTTP request models of the feature
├── DependencyInjection/
│   └── <Module>Module.cs                   Add<Module>Module(): Application, Infrastructure, controllers
└── <Module>.Api.csproj
~~~

Controllers bind HTTP input, call `ISender.Send` with `CancellationToken`, and return the result through the existing Result mapping. They do not run EF queries, create repositories, use Infrastructure types, or contain domain rules. Access attributes take policy names from `Identity.Contracts`.

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
  → controller → ISender
  → request logging → exceptions → performance → validation → domain events
  → handler → Result to HTTP mapping
~~~

The host is the composition root. It registers modules and middleware but does not implement business use cases. `Host.Api` does not run migrations at startup; a separate [migration job](../../operations/data/migrations-and-seeding.md) handles them.

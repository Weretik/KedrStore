# API and host structure

## API module structure

~~~text
<Module>.Api/
├── Controllers/
│   └── <Area>Controller.cs
├── GlobalUsing.cs
└── <Module>.Api.csproj
~~~

Controllers bind HTTP input, call ISender.Send with CancellationToken and return the established Result mapping. They do not query EF, construct repositories or contain domain rules.

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
  → exception / developer diagnostics
  → CORS, HTTPS, Serilog request logging, rate limiter
  → authentication → authorization
  → controller → ISender
  → request logging → exception → performance → validation → domain events
  → handler → Result-to-HTTP mapping
~~~

Host is the composition root. It may register modules and middleware but must not implement a business use case.

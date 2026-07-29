# Infrastructure structure

## Responsibility

Infrastructure is the adapter layer. It implements Application contracts and persists/communicates with technology-specific systems.

## Target structure per module

~~~text
<Module>.Infrastructure/
├── DataBase/
│   └── <Module>DbContext.cs
├── Configurations/          EF IEntityTypeConfiguration classes
├── Migrations/              EF migrations and model snapshot
├── Repositories/            EF repository implementations
├── Projections/             read-model rebuilding and persistence
├── Integrations/
│   └── <System>/            HTTP/SOAP/queue adapter implementations
├── Services/                storage, export, notification adapters
├── DependencyInjection/     Add<Module>... extension methods
└── <Module>.Infrastructure.csproj
~~~

## Adapter visualisation

~~~text
Application abstraction
          ▲
          │ implemented by
Infrastructure adapter
     ├── EF Core DbContext ──► PostgreSQL
     ├── external client ────► 1C / Telegram / storage
     └── projection builder ─► read model
~~~

## Rules

- EF configurations and migrations never move into Domain or Application.
- Register adapters through the existing module DI extension.
- Infrastructure may reference Application and Domain; the reverse is forbidden.
- Keep external payload mapping at the adapter boundary and never leak generated client types inward.

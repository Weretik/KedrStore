# Infrastructure structure

## Responsibility

Infrastructure is the adapter layer. It implements Application contracts, persists data, and interacts with specific technologies.

## Target module structure

~~~text
<Module>.Infrastructure/
├── DataBase/
│   └── <Module>DbContext.cs
├── Configurations/          EF IEntityTypeConfiguration classes
├── Migrations/              EF migrations and model snapshot
├── Repositories/            EF repository implementations
├── Projections/             read-model rebuild and persistence
├── Integrations/
│   └── <System>/            HTTP/SOAP/queue adapter implementations
├── Services/                storage, export, notification adapters
├── DependencyInjection/     Add<Module>... extension methods
└── <Module>.Infrastructure.csproj
~~~

## Adapter diagram

~~~text
Application abstraction
          ▲
          │ implemented by
Infrastructure adapter
     ├── EF Core DbContext ──► PostgreSQL
     ├── external client ────► 1C / Telegram / storage
     └── projection build ──► read model
~~~

## Rules

- EF configurations and migrations do not move into Domain or Application.
- Register adapters through the module's existing DI extension.
- Infrastructure may reference Application and Domain; references in the opposite direction are forbidden.
- Keep external data transformation at the adapter boundary; do not pass generated client types into inner layers.

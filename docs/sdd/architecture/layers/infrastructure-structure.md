# Infrastructure structure

## Responsibility

Infrastructure is the adapter layer. It implements Application contracts, persists data, and interacts with specific technologies.

## Target module structure

Folders follow aggregates and external systems ([module rules](../../standards/module-rules.md#folder-structure-inside-a-module)).

~~~text
<Module>.Infrastructure/
├── Persistence/
│   ├── <Module>DbContext.cs
│   ├── <Module>DbContextFactory.cs   design-time factory
│   ├── Migrations/
│   └── <Aggregate>/                  entity configurations, repository, read queries of that aggregate
├── Integrations/
│   └── <System>/                     1C, CDN, Telegram: client, mapping, sync services
├── Contracts/
│   └── <Feature>/                    implementations of the module's own Contracts interfaces
├── DependencyInjection/              Add<Module>Infrastructure(), called by Add<Module>Module()
└── <Module>.Infrastructure.csproj
~~~

Do not create `Configurations/`, `Repositories/`, `Services/`, or `DataBase/` folders that collect several aggregates.

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
- Infrastructure may reference its own Application and Domain and other modules' `Contracts`; references in the opposite direction, and to another module's Application, Domain, Infrastructure, or `DbContext`, are forbidden.
- Keep external data transformation at the adapter boundary; do not pass generated client types into inner layers.

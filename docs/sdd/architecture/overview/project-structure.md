# Project and layer structure

## Solution map

~~~text
KedrStore.sln
│
├── src/
│   ├── Bootstrapper/
│   │   ├── Host.Api/                 composition root and HTTP host: registers modules only
│   │   └── Host.Jobs/                CLI host for jobs: picks the job, runs the owning module's code
│   ├── BuildingBlocks/
│   │   ├── BuildingBlocks.Domain/
│   │   ├── BuildingBlocks.Application/
│   │   ├── BuildingBlocks.Infrastructure/
│   │   ├── BuildingBlocks.Integrations.OneC/
│   │   └── BuildingBlocks.Api/
│   └── Modules/
│       └── <Module>/                 Catalog, Sales, Identity, Content, Media, Work
│           ├── <Module>.Api/             Endpoints/<Feature>/<UseCase>/ Minimal API endpoints, DependencyInjection/Add<Module>Module(), Map<Module>Endpoints()
│           ├── <Module>.Application/     Abstractions/, Features/<Feature>/Commands|Queries/<UseCase>/
│           ├── <Module>.Contracts/       what other modules may use: interfaces, DTOs, events
│           ├── <Module>.Domain/          <Aggregate>/
│           └── <Module>.Infrastructure/  Persistence/<Aggregate>/, Integrations/<System>/, Adapters/<Provider>/, Contracts/<Feature>/
│
├── tests/
│   ├── UnitTests/                    <Module>/Domain|Application/...
│   ├── IntegrationTests/             <Module>/Api|Persistence|Integrations/..., Platform/
│   └── ArchitectureTests/            <Rule>/
└── docs/
    ├── sdd/                         current documentation
    └── legacy/                      archive only
~~~

Folder rules for every layer and for tests: [module rules](../../standards/module-rules.md#folder-structure-inside-a-module). Current gaps are listed in its [known deviations](../../standards/module-rules.md#known-deviations).

## Dependency diagram

~~~text
                         Host.Api / Host.Jobs
                                 │ Add<Module>Module()
                                 ▼
        <Module>.Api ──(registration only)──► <Module>.Infrastructure
             │                                   │
             └──────────────┐     ┌──────────────┘
                            ▼     ▼
                      <Module>.Application ──► <Other>.Contracts
                            │
                            ▼
                       <Module>.Domain

<Module>.Contracts — the only part of a module another module may reference.
It is implemented inside the module (Infrastructure/Contracts/<Feature>/).
~~~

Every business module has all five projects; keep the dependency direction. `BuildingBlocks.Integrations.OneC` is a separate shared transport project described in the [BuildingBlocks guide](../platform/building-blocks/README.md).

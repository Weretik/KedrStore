# Project and layer structure

## Solution map

~~~text
KedrStore.sln
│
├── src/
│   ├── Bootstrapper/
│   │   ├── Host.Api/                 composition root and HTTP host
│   │   └── Host.Jobs/                CLI host for jobs
│   ├── BuildingBlocks/
│   │   ├── BuildingBlocks.Domain/
│   │   ├── BuildingBlocks.Application/
│   │   ├── BuildingBlocks.Infrastructure/
│   │   └── BuildingBlocks.Api/
│   ├── Catalog/
│   │   ├── Catalog.Api/
│   │   ├── Catalog.Application/
│   │   ├── Catalog.Contracts/
│   │   ├── Catalog.Domain/
│   │   └── Catalog.Infrastructure/
│   ├── Sales/
│   │   ├── Sales.Api/
│   │   ├── Sales.Application/
│   │   ├── Sales.Domain/
│   │   └── Sales.Infrastructure/
│   └── Identity/
│       ├── Identity.Api/
│       ├── Identity.Application/
│       ├── Identity.Domain/
│       └── Identity.Infrastructure/
│
├── tests/
│   ├── UnitTests/
│   ├── IntegrationTests/
│   └── ArchitectureTests/
└── docs/
    ├── sdd/                         current documentation
    └── legacy/                      archive only
~~~

## Dependency diagram

~~~text
                         Host.Api / Host.Jobs
                          │        │
             ┌────────────┘        └─────────────┐
             ▼                                   ▼
        <Module>.Api                     <Module>.Infrastructure
             │                                   │
             └──────────────┐     ┌──────────────┘
                            ▼     ▼
                      <Module>.Application
                            │
                            ▼
                       <Module>.Domain

<Module>.Contracts — stable DTO boundary for relevant consumers.
~~~

Not every module needs every project, but preserve the dependency direction. `BuildingBlocks.Integrations.OneC` is a separate shared transport project described in the [BuildingBlocks guide](../platform/building-blocks/README.md).

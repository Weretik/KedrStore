# Project and layer structures

## Solution map

~~~text
KedrStore.sln
│
├── src/
│   ├── Bootstrapper/
│   │   ├── Host.Api/                 composition root and HTTP host
│   │   └── Host.Jobs/                job host
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
    ├── sdd/                         active documentation
    └── legacy/                      archive only
~~~

## Dependency visualisation

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

<Module>.Contracts is a stable DTO boundary consumed where needed.
~~~

Every module does not have to use every project, but it must preserve the direction shown above.

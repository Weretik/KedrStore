# Структура проєкту й шарів

## Карта solution

~~~text
KedrStore.sln
│
├── src/
│   ├── Bootstrapper/
│   │   ├── Host.Api/                 composition root і HTTP host
│   │   └── Host.Jobs/                CLI host для jobs
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
    ├── sdd/                         чинна документація
    └── legacy/                      лише архів
~~~

## Схема залежностей

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

<Module>.Contracts — стала межа DTO для потрібних споживачів.
~~~

Не кожному модулю потрібен кожен із цих проєктів, але напрям залежностей потрібно зберігати. `BuildingBlocks.Integrations.OneC` — окремий спільний транспортний проєкт, описаний у [довіднику BuildingBlocks](../platform/building-blocks/README.md).

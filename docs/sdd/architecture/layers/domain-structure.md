# Domain structure

## Responsibility

Domain owns the business model and its invariants. It must be understandable without ASP.NET Core, EF Core, SQL, HTTP or external client knowledge.

## Target structure per module

~~~text
<Module>.Domain/
├── Entities/                 aggregates and entities
│   └── <Area>/
├── ValueObjects/             validated conceptual values and typed IDs
├── Errors/                   domain error factories/codes
├── Events/                   domain events
├── Services/                 only pure domain services
├── Specifications/           domain-level specifications when applicable
├── Constants/
└── <Module>.Domain.csproj
~~~

## Placement diagram

~~~text
Application command
        │ calls behaviour
        ▼
Aggregate / Entity ───► Value object
        │ enforces invariant       │ validates concept
        ├──► Domain error
        └──► Domain event (collected, not dispatched here)
~~~

## Rules

- Factories and behaviour methods enforce invariants.
- Use a value object when primitive values would permit an invalid state.
- Domain events describe a completed business fact; they are collected by the entity.
- No request DTO, DbSet, EF configuration, repository implementation, HTTP call or logging dependency belongs here.

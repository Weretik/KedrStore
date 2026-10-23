# Domain structure

## Responsibility

Domain owns the business model and its invariants. It can be understood without knowing ASP.NET Core, EF Core, SQL, HTTP, or external clients.

## Target module structure

~~~text
<Module>.Domain/
├── Entities/                 aggregates and entities
│   └── <Area>/
├── ValueObjects/             validated values and typed IDs
├── Errors/                   domain error factories/codes
├── Events/                   domain events
├── Services/                 pure domain services only
├── Specifications/           domain specifications when needed
├── Constants/
└── <Module>.Domain.csproj
~~~

## Placement diagram

~~~text
Application command
        │ invokes behavior
        ▼
Aggregate / Entity ───► Value object
        │ enforces invariant      │ validates concept
        ├──► Domain error
        └──► Domain event (accumulated, not dispatched here)
~~~

## Rules

- Factories and behavior methods enforce invariants.
- Use a value object when a primitive allows an invalid state.
- A domain event describes a completed business fact; an entity accumulates it.
- Request DTOs, `DbSet`, EF configuration, repository implementations, HTTP calls, and logging dependencies do not belong here.

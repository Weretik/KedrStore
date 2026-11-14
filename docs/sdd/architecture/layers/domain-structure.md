# Domain structure

## Responsibility

Domain owns the business model and its invariants. It can be understood without knowing ASP.NET Core, EF Core, SQL, HTTP, or external clients.

## Target module structure

Folders follow aggregates, not kinds of class ([module rules](../../standards/module-rules.md#folder-structure-inside-a-module)).

~~~text
<Module>.Domain/
├── Orders/                   one folder per aggregate
│   ├── Order.cs              aggregate root
│   ├── OrderLine.cs          child entity
│   ├── OrderId.cs            typed ID and other value objects
│   ├── OrderStatus.cs        enums
│   ├── OrderErrors.cs        domain errors
│   ├── OrderPlaced.cs        domain events
│   └── Lines/                only when the aggregate grows: a sub-concept, not a type
├── Customers/
├── Money/                    a value object shared by several aggregates, named after the concept
└── <Module>.Domain.csproj
~~~

Do not create `Entities/`, `ValueObjects/`, `Enums/`, `Errors/`, `Events/`, or `Constants/` folders.

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

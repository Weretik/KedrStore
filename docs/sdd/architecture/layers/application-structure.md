# Application structure

## Responsibility

Application executes one use case at a time. It coordinates Domain and abstractions; it does not own persistence or transport implementation.

## Target structure per feature

~~~text
<Module>.Application/
├── Features/
│   └── <Area>/
│       └── <UseCase>/
│           ├── <UseCase>Command.cs | <UseCase>Query.cs
│           ├── <UseCase>CommandHandler.cs | <UseCase>QueryHandler.cs
│           ├── DTOs/               private use-case DTOs, only when needed
│           ├── Validators/
│           │   └── <UseCase>Validator.cs
│           └── Extensions/         cohesive LINQ/mapping helpers only
├── Contracts/
│   ├── Persistence/                DbContext/read/repository abstractions
│   ├── Integrations/               external-service abstractions
│   └── Services/
├── Integrations/                   application orchestration of external flows
├── Jobs/                           use-case job logic, never scheduler wiring
└── <Module>ApplicationAssemblyMarker.cs
~~~

## Use-case visualisation

~~~text
Command or Query
       │
       ▼
FluentValidation validator ── invalid ──► Result.Invalid
       │ valid
       ▼
Handler ──► Domain aggregate / Application abstraction
       │
       └──► Result.Success | Result.NotFound | Result.Conflict | Result.Invalid
~~~

## Rules

- Commands write; queries read only.
- A handler has one use case and receives only abstractions.
- A query uses projection/no-tracking as appropriate.
- Validators own input validation; Domain owns business invariants.
- Put a public cross-module DTO in Module.Contracts, not beside a handler.

# Application structure

## Responsibility

Application executes one use case at a time. It coordinates Domain and abstractions but does not own persistence or transport implementations.

## Target feature structure

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
│           └── Extensions/         only related LINQ/mapping helpers
├── Contracts/
│   ├── Persistence/                DbContext/read/repository abstractions
│   ├── Integrations/               external service abstractions
│   └── Services/
├── Integrations/                   external flow coordination
├── Jobs/                           use-case job logic, not scheduler wiring
└── <Module>ApplicationAssemblyMarker.cs
~~~

## Use-case diagram

~~~text
Command or Query
       │
       ▼
FluentValidation validator ── invalid ──► Result.Invalid
       │ valid input
       ▼
Handler ──► Domain aggregate / Application abstraction
       │
       └──► Result.Success | Result.NotFound | Result.Conflict | Result.Invalid
~~~

## Rules

- Commands change state; queries only read.
- A handler executes one use case and depends only on abstractions.
- A query uses a no-tracking projection when appropriate.
- Validators own input checks; Domain owns business invariants.
- Put a public cross-module DTO in Module.Contracts, not beside the handler.

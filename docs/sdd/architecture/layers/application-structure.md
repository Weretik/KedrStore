# Application structure

## Responsibility

Application executes one use case at a time. It coordinates Domain and abstractions but does not own persistence or transport implementations.

## Target feature structure

Folders follow features and CQRS ([module rules](../../standards/module-rules.md#folder-structure-inside-a-module)).

~~~text
<Module>.Application/
├── Abstractions/                       ports: repositories, read stores, unit of work, external services
├── Features/
│   └── <Feature>/                      for example Redirects, Products, Orders
│       ├── Commands/
│       │   └── <UseCase>/              SaveRedirect/
│       │       ├── <UseCase>Command.cs
│       │       ├── <UseCase>CommandHandler.cs
│       │       └── <UseCase>CommandValidator.cs
│       ├── Queries/
│       │   ├── <UseCase>/              GetRedirects/ (admin)
│       │   └── <PublicUseCase>/        GetPublicRedirects/ (site) in the same feature
│       │       ├── <UseCase>Query.cs
│       │       ├── <UseCase>QueryHandler.cs
│       │       ├── <UseCase>QueryValidator.cs
│       │       └── <UseCase>Response.cs
│       ├── EventHandlers/              domain or integration event handlers of the feature
│       └── Shared/                     DTOs, mapping, policies used by several use cases of the feature
└── <Module>ApplicationAssemblyMarker.cs
~~~

Do not create `Common/`, `DTOs/`, `Validators/`, `Extensions/`, or `Public…` feature folders. The name `Contracts` is reserved for the `<Module>.Contracts` project.

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

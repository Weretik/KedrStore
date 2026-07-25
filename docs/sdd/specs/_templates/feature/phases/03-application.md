# Phase 03: Application use cases

**Status:** draft  
**Depends on:** phases 01 and 02 as applicable  
**Blocks:** API exposure

## Outcome

Each required operation is represented by a focused CQRS use case with explicit validation and Result outcomes.

## Required operation inventory

Mark only operations actually required:

| Operation | Command/query | Result | Authorization/ownership | Needed? |
| --- | --- | --- | --- | --- |
| Create | command | ID/status | | |
| Get list | query | paged projection | | |
| Get by ID | query | DTO/not found | | |
| Update | command | status/DTO | | |
| Delete | command | status | | |
| Other | | | | |

## Design

- Handler per use case:
- Request/response DTO ownership: private DTO or Module.Contracts:
- FluentValidation rules:
- Domain/business rules not covered by validator:
- Ardalis.Result statuses:
- IUnitOfWork/transaction boundary:
- Ardalis.Specification: write specification, read specification, or direct read projection; explain choice:
- Cancellation propagation:
- Assembly marker impact:
  - Existing Catalog, Sales or Identity module: confirm the existing ApplicationAssemblyMarker is already scanned.
  - New Application module: create <Module>ApplicationAssemblyMarker and add it to Host.Api MediatorRegistrationExtensions assemblies and FluentValidationRegistrationExtensions scanning.

## File plan

~~~text
<Module>.Application/Features/<Area>/<UseCase>/
├── Command or Query
├── Handler
├── Validators/
├── DTOs/
└── Extensions/
~~~

## Acceptance criteria

- [ ] Commands do not hide in queries; queries do not write.
- [ ] Every input rule is validated before the handler.
- [ ] Domain invariants remain in Domain.
- [ ] Read model is projection-focused and write path uses the existing transaction convention.
- [ ] Mediator can discover the handler and FluentValidation can discover the validator through the correct application assembly marker.

## Verification

- Unit tests:
- Handler/integration tests:
- Risks:

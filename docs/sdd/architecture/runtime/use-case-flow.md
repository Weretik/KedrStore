# Use-case flow

## Standard HTTP flow

~~~text
HTTP request
  → Controller binding
  → ISender.Send(request, cancellationToken)
  → Mediator pipeline
  → Command/query handler
  → Domain + application abstractions
  → Infrastructure adapter / database
  → Ardalis.Result
  → ToActionResult HTTP response
~~~

## CQRS rules

- A command changes state and returns only the needed identifier, status or Result.
- A query does not change state and projects only required fields.
- Do not hide writes in query handlers or background read operations.
- Propagate CancellationToken through every async operation.
- Make retry-sensitive commands idempotent when duplicate side effects are possible. Document the key, scope and retention if an endpoint uses Idempotency-Key.

## Feature placement

~~~text
<Module>.Application/Features/<Area>/<UseCase>/
  <UseCase>Command|Query.cs
  <UseCase>Command|QueryHandler.cs
  DTOs/                 (only if private to the use case)
  Validators/
  Extensions/           (only cohesive query/mapping helpers)
~~~

Split a file when it has independently evolving responsibilities. Do not create generic dumping folders such as Common, Helpers or Utils.

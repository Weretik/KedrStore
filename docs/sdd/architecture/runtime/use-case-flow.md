# Use-case flow

## Typical HTTP flow

~~~text
HTTP request
  → Controller binding
  → ISender.Send(request, cancellationToken)
  → Mediator pipeline
  → Command/query handler
  → Domain + Application abstractions
  → Infrastructure adapter / database
  → Ardalis.Result
  → HTTP response through ToActionResult
~~~

## CQRS rules

- A command changes state and returns only the needed identifier, status, or `Result`.
- A query does not change state and projects only the required fields.
- Do not hide writes in query handlers or background read operations.
- Pass `CancellationToken` through every asynchronous operation.
- Make retry-sensitive commands idempotent when duplicate side effects are possible. For endpoints using `Idempotency-Key`, document its key, scope, and retention period.

## Feature placement

~~~text
<Module>.Application/Features/<Area>/<UseCase>/
  <UseCase>Command|Query.cs
  <UseCase>Command|QueryHandler.cs
  DTOs/                 (private to this use case only)
  Validators/
  Extensions/           (related query/mapping helpers only)
~~~

Split a file when its parts have separate reasons to change. Do not create catch-all `Common`, `Helpers`, or `Utils` folders.

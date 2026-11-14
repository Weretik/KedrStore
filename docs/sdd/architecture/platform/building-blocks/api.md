# BuildingBlocks.Api

## Purpose

BuildingBlocks.Api turns an Ardalis `Result` into the HTTP answer and every error into RFC 9457 `ProblemDetails` ([platform spec 005](../../../specs/platform/005-minimal-api-problem-details/README.md)).

- [ResultToActionResult](../../../../../src/BuildingBlocks/BuildingBlocks.Api/Results/ResultToActionResult.cs): `ToActionResult` for controllers and `ToProblem` for a failure an action answers itself.
- [ResultProblems](../../../../../src/BuildingBlocks/BuildingBlocks.Api/Results/ResultProblems.cs): the error body.
- [ProblemCodes](../../../../../src/BuildingBlocks/BuildingBlocks.Api/Results/ProblemCodes.cs): the `code` words.
- [ProblemDetailsSetup](../../../../../src/BuildingBlocks/BuildingBlocks.Api/Results/ProblemDetailsSetup.cs): `AddApiProblemDetails()` adds `code` and `traceId` to errors that do not come from a result (model binding, empty 401/403/404 through status code pages, unhandled exceptions in [GlobalExceptionHandler](../../../../../src/BuildingBlocks/BuildingBlocks.Api/Errors/GlobalExceptionHandler.cs)).

## Mapping

~~~text
ResultStatus.Ok            → 200 with the value (no body for a Result without value)
ResultStatus.Created       → 201 with the value
ResultStatus.NoContent     → 204
ResultStatus.Invalid       → 400 problem, code = the rule's own error code or "validation", errors = { field: [messages] }
ResultStatus.Unauthorized  → 401 problem, code "unauthorized"
ResultStatus.Forbidden     → 403 problem, code "forbidden"
ResultStatus.NotFound      → 404 problem, code "not_found"
ResultStatus.Conflict      → 409 problem, code "conflict"
ResultStatus.Unavailable   → 503 problem, code "unavailable"
Error / CriticalError      → 500 problem, code "unexpected"
~~~

Every problem is `application/problem+json` with `type`, `title` (Ukrainian), `status`, `detail` (the first message, Ukrainian), `code` and `traceId`. A FluentValidation built-in error code (`NotEmptyValidator` and the like) is not a client code, so such a 400 says `validation`.

## Rule

After `ISender.Send`, controllers call `ToActionResult`. An action that answers success itself calls `this.ToProblem(result)` for the failure. Do not map `Result` statuses or build error bodies in an endpoint; the only typed error bodies are the ones the OpenAPI document declares (order checkout 409 with `quote`, site refresh 502/503, site model 404 redirect).

# BuildingBlocks.Api

## Purpose

BuildingBlocks.Api contains the MVC mapping from Ardalis.Result to HTTP `ActionResult`.

## Mapping

~~~text
ResultStatus.Ok           → 200 OK with value
ResultStatus.NotFound     → 404
ResultStatus.Invalid      → 400 with validation errors
ResultStatus.Conflict     → 409 with errors
ResultStatus.Forbidden    → 403
ResultStatus.Unauthorized → 401
other/unmapped            → 500
~~~

## Rule

After `ISender.Send`, controllers call the existing `ToActionResult` extension method. Do not map `Result` statuses manually in each endpoint unless the documented API contract requires an extension already adopted by the project.

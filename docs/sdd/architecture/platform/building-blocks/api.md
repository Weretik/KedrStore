# BuildingBlocks.Api

## Purpose

BuildingBlocks.Api contains the MVC mapping from Ardalis.Result to HTTP ActionResult.

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

Controllers call the established ToActionResult extension after ISender.Send. They do not manually reinterpret Result statuses per endpoint, except when a documented API contract requires an existing project convention extension.

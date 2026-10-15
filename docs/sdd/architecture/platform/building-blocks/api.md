# BuildingBlocks.Api

## Призначення

BuildingBlocks.Api містить MVC-відображення Ardalis.Result у HTTP `ActionResult`.

## Відображення

~~~text
ResultStatus.Ok           → 200 OK with value
ResultStatus.NotFound     → 404
ResultStatus.Invalid      → 400 with validation errors
ResultStatus.Conflict     → 409 with errors
ResultStatus.Forbidden    → 403
ResultStatus.Unauthorized → 401
other/unmapped            → 500
~~~

## Правило

Після `ISender.Send` контролери викликають чинний метод розширення `ToActionResult`. Не перевизначай статуси `Result` вручну в кожній кінцевій точці, крім випадку, коли документований API-контракт потребує вже прийнятого в проєкті розширення.

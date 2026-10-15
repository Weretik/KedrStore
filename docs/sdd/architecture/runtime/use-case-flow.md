# Потік сценарію використання

## Звичайний HTTP-потік

~~~text
HTTP request
  → прив'язка в Controller
  → ISender.Send(request, cancellationToken)
  → Mediator pipeline
  → Command/query handler
  → Domain + абстракції Application
  → адаптер Infrastructure / БД
  → Ardalis.Result
  → HTTP-відповідь через ToActionResult
~~~

## Правила CQRS

- Команда змінює стан і повертає лише потрібний ідентифікатор, статус або `Result`.
- Запит не змінює стан і проєктує тільки потрібні поля.
- Не приховуй операції запису в обробниках запитів або фонових операціях читання.
- Передавай `CancellationToken` крізь кожну асинхронну операцію.
- Роби чутливі до повторів команди ідемпотентними, якщо можливі подвійні побічні ефекти. Для кінцевої точки з `Idempotency-Key` опиши ключ, обсяг і строк зберігання.

## Розміщення фічі

~~~text
<Module>.Application/Features/<Area>/<UseCase>/
  <UseCase>Command|Query.cs
  <UseCase>Command|QueryHandler.cs
  DTOs/                 (лише приватні для цього сценарію)
  Validators/
  Extensions/           (лише пов'язані query/mapping helpers)
~~~

Поділяй файл, коли його частини мають окремі причини для змін. Не створюй папок-звалищ `Common`, `Helpers` або `Utils`.

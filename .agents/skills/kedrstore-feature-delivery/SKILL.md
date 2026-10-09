---
name: kedrstore-feature-delivery
description: "Use when implementing or continuing an accepted KedrStore backend feature specification at feature, phase, scenario, or task scope."
---

# KedrStore Feature Delivery

Реалізуй лише погоджений scope і підтримуй specification evidence синхронним із кодом.

## Workflow

1. Прочитай кореневий `AGENTS.md`, усі artifacts feature, applicable architecture/standards та AI workflow `01`–`04`.
2. Зафіксуй authorized scope: whole feature, phase, `SC-*`, `TS-*` або `EN-*`. Перевір readiness і dependency graph.
3. Перевір baseline фактичного коду та module ownership. Системні findings не маскуй локальним workaround; маршрутизуй у remediation plan.
4. Для кожного behavior slice виконай Red → Green → Refactor → Regression на найвужчому рівні, який доводить ризик.
5. Розміщуй business rules у Domain/Application, EF та integrations в Infrastructure, transport у Module.Api, composition у hosts. Не створюй міжмодульних залежностей на internals.
6. Для schema/seed changes застосуй `$kedrstore-database-change`; для HTTP contract work — `$kedrstore-api-contract-sync`.
7. Після task онови status, traceability, exact commands/results, deviations і residual risks. Перед completion застосуй change verification та code audit.

## Guardrails

- Не реалізуй незатверджену поведінку й не обходь readiness gates.
- Не редагуй applied migrations і не запускай state-changing operations проти непогодженої БД.
- Не commit, push або створюй PR без прямого запиту.
- Зберігай unrelated user changes.

## Example

`Use $kedrstore-feature-delivery to implement SC-003 from docs/sdd/specs/catalog/004-product-archive/.`

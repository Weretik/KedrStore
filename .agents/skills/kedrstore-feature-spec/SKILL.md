---
name: kedrstore-feature-spec
description: "Use when creating, extending, or incrementally migrating a KedrStore backend feature specification before implementation."
---

# KedrStore Feature Specification

Створюй behavior-first SDD-специфікацію до зміни runtime code. Цей skill не реалізує feature.

## Workflow

1. Прочитай кореневий `AGENTS.md`, `docs/sdd/architecture/README.md`, релевантні standards і `docs/sdd/specs/_templates/README.md`.
2. Визнач owning module: `catalog`, `sales`, `identity`, `platform` або інший наявний module. Не створюй нову межу модуля без явного architecture decision.
3. Створи або доповни `docs/sdd/specs/<module>/<NNN>-<feature-slug>/` з observable goal, in/out scope, rules `R-*`, scenarios `SC-*`, design, risks, tasks і traceability.
4. Для persistent model до коду зафіксуй identifier strategy, integrity, migration baseline, rollout і rollback. Прочитай `identifier-strategy.md` та `database-rules.md`.
5. Для HTTP behavior узгодь route, method, operationId, DTO, errors, authorization, idempotency і compatibility; зв'яжи майбутній OpenAPI файл у `docs/sdd/contracts/`.
6. Розбий delivery на dependency-ready `TS-*` і `EN-*`; кожна behavior task має визначати Red, Green, Refactor, Regression та evidence.
7. Перевір readiness checklist і unresolved `[NEEDS CLARIFICATION: ...]`. Зупинись після specification artifacts; implementation потребує окремого запиту.

## Guardrails

- Не вигадуй business rules, database state, public contracts або зовнішні системи.
- Не копіюй stable standards у feature; посилайся на них.
- Не перенумеровуй завершені IDs і не переписуй historical evidence без потреби.
- Не запускай міграції, seeders або jobs у planning scope.

## Example

`Use $kedrstore-feature-spec to prepare catalog feature 004-product-archive. Goal: administrators can archive a product without deleting history.`

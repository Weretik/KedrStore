---
name: kedrstore-feature-orchestrator
description: "Use when an accepted KedrStore feature specification must be delivered with multiple agents and dependency-aware integration."
---

# KedrStore Feature Orchestrator

Координуй multi-agent delivery лише коли користувач явно попросив кількох agents і specification уже прийнята.

## Required input

- Exact path до feature specification.
- Accepted implementation scope.
- Явний запит на multi-agent work.
- Окреме рішення щодо commits; за замовчуванням agents не commit, push або create PR.

## Workflow

1. Прочитай `AGENTS.md`, feature artifacts, AI workflow, architecture і standards. Підтвердь readiness та відсутність unresolved blockers.
2. Побудуй dependency graph для in-scope `SC-*`, `TS-*` і `EN-*`.
3. Створи work packets за `references/delegation-contract.md`. Один owner відповідає за кожен mutable path; shared evidence залишається coordinator-owned.
4. Паралель лише незалежні packets без спільних mutable files. Database migrations, aggregate OpenAPI, host composition і shared feature records інтегруй послідовно.
5. Кожен implementation packet виконується через `$kedrstore-feature-delivery`; contract і database packets додатково використовують відповідні skills.
6. Після кожної хвилі перевір scoped diffs, ownership і narrow tests. Coordinator оновлює shared traceability/evidence.
7. На integrated snapshot запусти `$kedrstore-change-verification` і незалежний `$kedrstore-code-audit`, потім підготуй `$kedrstore-pr-handoff`.

## Stop conditions

Зупинись при неготовій specification, overlapping path ownership, непогодженому contract/schema change, required check failure або open in-scope audit finding.

## Example

`Use $kedrstore-feature-orchestrator to deliver the accepted catalog feature with multiple agents; stop before commits and PR creation.`

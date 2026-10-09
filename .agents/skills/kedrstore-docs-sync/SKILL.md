---
name: kedrstore-docs-sync
description: "Use when checking or repairing drift between KedrStore code and versioned architecture, standards, contracts, operations, or feature documentation."
---

# KedrStore Documentation Sync

Знайди й виправ лише підтверджений drift між repository truth та canonical docs.

## Workflow

1. Визнач scope через user request, `git status` і scoped diff. Прочитай `AGENTS.md` та relevant docs indexes.
2. Визнач canonical source: project references/config for architecture, source/runtime tests for behavior, EF migrations for schema lineage, OpenAPI for public contracts, feature artifacts for accepted intent.
3. Порівняй source з `docs/sdd/architecture/`, `standards/`, `contracts/`, `operations/` і relevant `specs/`.
4. Класифікуй drift як stale, missing, contradictory або historical-only. Audit-only запит повертає findings; repair scope вносить мінімальні changes.
5. Не документуй planned behavior як implemented і не переписуй historical evidence. Contract drift маршрутизуй через `$kedrstore-api-contract-sync`.
6. Перевір changed links/paths, skill references і `git diff --check`; для behavior claims запусти відповідні repository checks.

## Guardrails

- `docs/legacy/` є historical context, не source of truth.
- Не роби broad rewrite, якщо drift локальний.
- Зберігай stable IDs та unrelated user changes.

## Example

`Use $kedrstore-docs-sync to compare Host.Jobs commands with operations docs and repair confirmed drift.`

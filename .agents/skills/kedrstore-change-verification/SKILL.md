---
name: kedrstore-change-verification
description: "Use when KedrStore code, tests, contracts, migrations, documentation, or build configuration changed and fresh completion evidence is required."
---

# KedrStore Change Verification

Обери найменший risk-adequate набір перевірок і надай свіже evidence.

## Workflow

1. Переглянь `git status`, scoped diff і diff stat; відокрем task changes від unrelated worktree changes.
2. Зістав ризик із test level за `docs/sdd/standards/testing-rules.md`: Unit, Integration, Architecture або contract/API tests.
3. Спочатку запускай focused project/test filters, потім required regression. Для feature completion, де застосовно, виконай:
   - `dotnet restore KedrStore.sln`
   - `dotnet build KedrStore.sln --no-restore`
   - `dotnet test KedrStore.sln --no-build`
4. Для layer/reference changes обов'язково включи `tests/ArchitectureTests`. Для EF/OpenAPI/HTTP/jobs — релевантні IntegrationTests і documented operational checks.
5. Для migration/seed change перевір generated migration, empty disposable PostgreSQL path, startup/job logs та rollout/rollback evidence без доступу до непогодженої БД.
6. Для docs-only change перевір links, paths, skill structure і `git diff --check`; runtime suite не видавай за обов'язкову без runtime impact.
7. Повідом exact commands, exit codes, test counts, warnings, skipped та unavailable checks.

## Guardrails

- Не називай роботу passing, якщо required check не виконано.
- Не виправляй unrelated failures без окремого scope.
- Skipped Docker integration test не є доказом PostgreSQL behavior.

## Example

`Use $kedrstore-change-verification to verify the current Identity session changes and report exact evidence.`

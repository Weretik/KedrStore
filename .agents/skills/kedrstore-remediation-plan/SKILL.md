---
name: kedrstore-remediation-plan
description: "Use when confirmed KedrStore architecture findings span multiple files, modules, features, or boundaries and need a separate remediation specification."
---

# KedrStore Remediation Plan

Перетвори systemic findings на dependency-ordered SDD plan; не реалізуй його в цьому skill.

## Workflow

1. Прочитай source audit, `AGENTS.md`, affected architecture/standards і фактичний код.
2. Підтвердь root cause та affected boundaries; відокрем symptoms від systemic cause.
3. Створи окрему remediation specification у доречному module/platform scope під `docs/sdd/specs/`, використовуючи feature structure без порожніх artifacts.
4. Зв'яжи `AF-*` з architecture requirements, `EN-*` prerequisites і remediation tasks. Збережи compatibility та sequencing constraints.
5. Для кожної task вкажи exact source/target paths, dependencies, migration order, verification, rollback і вплив на API/data.
6. Перевір readiness та ownership; implementation починай лише після окремого погодження.

## Guardrails

- Не створюй remediation plan для локального one-file fix.
- Не приховуй breaking changes під словом refactor.
- Створення plan саме по собі не закриває source findings.
- Не змінюй runtime code у planning scope.

## Example

`Use $kedrstore-remediation-plan to turn AF-004 and AF-006 into a platform remediation specification.`

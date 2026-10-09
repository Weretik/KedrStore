---
name: kedrstore-code-audit
description: "Use when reviewing KedrStore architecture, module boundaries, implementation quality, or feature delivery evidence without automatically fixing findings."
---

# KedrStore Code Audit

Проводь evidence-based review. Audit-only запит не дозволяє змінювати runtime code.

## Workflow

1. Визнач scope; прочитай `AGENTS.md`, relevant feature, architecture, standards і contracts.
2. Перевір project references і compiled/source dependencies, а не лише назви каталогів.
3. Перевір Domain invariants, Application orchestration/validation, Infrastructure persistence/integrations, API mapping/authorization, host composition, tests і docs.
4. Для data/API changes окремо звір identifier choice, transactions, query shape, migration safety, DTO/errors/security та OpenAPI.
5. Запиши actionable findings зі stable `AF-*`, severity, status, exact path, evidence, impact, ownership, proposed target, dependencies і verification.
6. Feature-local findings зберігай у feature audit artifact; systemic findings передавай у `$kedrstore-remediation-plan`.
7. Open, blocked або planned in-scope findings явно познач як delivery blockers.

## Guardrails

- Не роби висновок лише зі structure diagram або documentation claim.
- Не занижуй severity через відсутність готового fix.
- Не закривай finding без fresh evidence.
- Не змішуй audit із implementation без окремого запиту.

## Example

`Use $kedrstore-code-audit to audit the Sales pricing feature and report findings only.`

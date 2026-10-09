---
name: kedrstore-pr-handoff
description: "Use when completed KedrStore work needs review-ready commit grouping, branch naming, PR text, verification evidence, and residual-risk handoff."
---

# KedrStore PR Handoff

Підготуй review-ready handoff. Не commit, push, create або merge PR без прямого запиту.

## Workflow

1. Переглянь `git status`, scoped diff, diff stat і recent log; відокрем unrelated changes.
2. Звір accepted scope, feature status, audit findings і fresh verification evidence.
3. Якщо commits дозволені, запропонуй batches за `git-commit-batching.md`; кожен commit має один reviewable intent і passing repository state. Model/configuration/migration не розривай штучно.
4. Запропонуй branch з префіксом `codex/`, imperative PR title і description: Summary, Scope, Database/API impact, Verification, Documentation, Residual risks.
5. Наведи exact commands/results, migration/rollback notes та reviewer focus areas.
6. Використовуй GitHub integration лише на явний запит створити або оновити PR.

## Guardrails

- Не включай secrets, local absolute paths або unverified claims.
- Не називай worktree clean без перевірки.
- Не включай unrelated user changes у commits або PR.

## Example

`Use $kedrstore-pr-handoff to prepare the handoff for the completed Catalog archive feature; do not create the PR.`

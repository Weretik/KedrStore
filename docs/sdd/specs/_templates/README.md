# SDD templates

This directory contains the reusable feature structure and the AI execution
workflow. Stable engineering rules remain in `docs/sdd/standards/`.

## Create a specification

Copy `feature/` to
`docs/sdd/specs/<module>/<NNN>-<feature-slug>/`. Replace placeholders and remove
unused optional documents or task templates; do not leave empty files. The
user-facing [`feature/USAGE.md`](feature/USAGE.md) explains what to send AI and
is not copied into the created feature.

```text
<NNN>-<feature-slug>/
├── README.md                 feature status and navigation
├── requirements/             rules and observable scenarios
├── design/                   feature-specific technical decisions
├── data-model.md             entities, relations, and integrity
├── contracts/                human-readable API/integration decisions
├── traceability.md           scenario-to-task and evidence mapping
├── tasks/                    planning and technical task files
└── checklist/                readiness and delivery gates
```

Give AI this request to prepare a specification without implementing it:

```text
Use `docs/sdd/specs/_templates/feature/`.
Create feature `<NNN>-<feature-slug>` in module `<module>`.
Goal: <observable user or business result>.
Scope: <included and explicitly excluded behavior>.
Prepare the specification; do not implement code yet.
```

Every behavior-implementing `TS-*` task must be authored and later executed in
the explicit order `Red -> Green -> Refactor -> Regression -> evidence`.
Planning and contract `EN-*` tasks may use a documented test-first exception;
they must name the replacement verification.

## Implement an accepted specification

Follow [the AI feature workflow](ai-feature-workflow/README.md). Authorize the
complete feature, a phase, selected `SC-*` scenarios, or named `TS-*`/`EN-*`
tasks. User-facing prompt examples are in
[`ai-feature-workflow/USAGE.md`](ai-feature-workflow/USAGE.md).

## Existing specifications

Use [the incremental migration guide](MIGRATION.md). Preserve completed task
IDs and verification history unless the user explicitly requests a full
migration.

`git/git-commit-batching.md` is independent guidance for commit planning.

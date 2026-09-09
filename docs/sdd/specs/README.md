# Feature specifications

This directory contains feature-specific requirements, design, contracts,
task records, and verification evidence. Every non-trivial behavior change
updates its specification before implementation.

## Layout

```text
docs/sdd/specs/
├── <module>/<NNN>-<feature>/     one feature specification
└── _templates/                   reusable structure and AI workflow
```

- Start new work with the [template catalog](_templates/README.md).
- Migrate existing specifications with the
  [incremental migration guide](_templates/MIGRATION.md).
- Keep machine-readable public API contracts in
  [`docs/sdd/contracts/`](../contracts/README.md).
- For catalog terms involving doors, hardware, Cosmos, or 1C product data, use
  the [product glossary](../../product/glossary.md).

## Lifecycle

Draft -> accepted -> in-progress -> verified -> completed.

An implementation must not silently diverge from an accepted specification.
Record a dated change note with the reason, scope impact, and new verification.

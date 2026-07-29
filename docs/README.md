# KedrStore documentation

This directory contains versioned engineering documentation for KedrStore. Backend documentation is maintained as docs-as-code alongside code changes.

## Backend SDD

- [Architecture](sdd/architecture/README.md) — the actual backend topology, modules, and layer boundaries.
- [Engineering rules](sdd/standards/README.md) — backend, API, data, security, testing, and delivery rules.
- [Operations](sdd/operations/README.md) — local startup, configuration, data, diagnostics, and jobs.
- [Specifications](sdd/specs/README.md) — feature specifications and SDD templates.
- [Product glossary](product/glossary.md) — stable catalog and door-industry terms.

- [API contracts](sdd/contracts/README.md) — versioned OpenAPI contracts and frontend integration guidance.

## Legacy documentation

[docs/legacy/](legacy/README.md) preserves ADRs, historical architecture notes, client PDFs, and feature notes that predate the SDD documentation set. It is retained for context and migration reference, but it is not the source of truth for new work.

## Placement principle

A stable rule belongs in `sdd/architecture` or `sdd/standards`. Decisions, requirements, contracts, tasks, and verification evidence for a specific feature belong in its folder under `sdd/specs/`.

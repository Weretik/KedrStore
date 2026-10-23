# KedrStore documentation

Versioned KedrStore engineering documentation is stored here. Backend documents are maintained alongside code changes. The [backend SDD entry point](sdd/README.md) explains the role of each section.

## Backend SDD

- [Architecture](sdd/architecture/README.md) — actual topology, modules, and layer boundaries.
- [Engineering rules](sdd/standards/README.md) — backend, API, data, security, testing, and delivery.
- [Operations](sdd/operations/README.md) — local startup, configuration, data, diagnostics, and jobs.
- [Specifications](sdd/specs/README.md) — requirements and evidence for individual features.
- [Feature map](sdd/features/README.md) — implemented scenarios by module, flows, and code entry points.
- [Product glossary](product/glossary.md) — stable terms for catalog, doors, and hardware.

- [API contracts](sdd/contracts/README.md) — versioned OpenAPI contracts and frontend integration.

## Historical documentation

The [archive](legacy/README.md) keeps older ADRs, architecture notes, client PDFs, and feature materials from before the current SDD set. It provides context and sources for comparison; it does not automatically define rules for new work.

## Information placement

Stable system descriptions belong in `sdd/architecture`, and normative rules in `sdd/standards`. Current feature structure and code navigation belong in `sdd/features/`; decisions, requirements, contracts, tasks, and verification evidence belong in the corresponding `sdd/specs/` directory.
